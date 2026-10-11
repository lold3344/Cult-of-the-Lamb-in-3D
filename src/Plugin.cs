using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace CotlCameraMove
{
    [BepInPlugin("local.cotl.camera-move", "COTL Camera Move", "1.7.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ConfigEntry<float> EyeLift;
        internal static ConfigEntry<float> EyeForward;
        internal static ConfigEntry<float> Fov;
        internal static ConfigEntry<float> AimOffset;
        internal static BepInEx.Logging.ManualLogSource Log;
        internal static bool Reported;

        void Awake()
        {
            Log = Logger;
            EyeLift = Config.Bind("Camera", "EyeLift", 2.2f, "Height above the floor, applied as -Z.");
            EyeForward = Config.Bind("Camera", "EyeForward", 0.45f, "Step forward so the sprite stays behind the camera.");
            Fov = Config.Bind("Camera", "Fov", 70f, "Perspective field of view.");
            AimOffset = Config.Bind("Camera", "AimOffset", 0f, "Extra degrees on PlayerFarming.AimAngle.");
            Harmony harmony = new Harmony("local.cotl.camera-move");
            Type follow = AccessTools.TypeByName("CameraFollowTarget");
            if (follow == null)
            {
                Logger.LogError("CameraFollowTarget missing");
                return;
            }
            harmony.Patch(AccessTools.Method(follow, "LateUpdate"), postfix: new HarmonyMethod(typeof(Apply), "After"));
            MethodInfo pre = AccessTools.Method(follow, "OnPreRender");
            if (pre != null)
                harmony.Patch(pre, postfix: new HarmonyMethod(typeof(Apply), "After"));
            Logger.LogInfo("player-view patched onto CameraFollowTarget");
        }
    }

    public static class Apply
    {
        static FieldInfo playerInstance;
        static FieldInfo aimAngle;
        static FieldInfo cameraBone;
        static Type playerType;

        public static void After(MonoBehaviour __instance)
        {
            if (__instance == null)
                return;
            Component player = Player();
            if (player == null)
            {
                Once("no PlayerFarming.Instance yet");
                return;
            }

            float angle = 0f;
            if (aimAngle != null)
                angle = Convert.ToSingle(aimAngle.GetValue(player));
            angle += Plugin.AimOffset.Value;
            float rad = angle * 0.0174532924f;
            Vector3 forward = new Vector3(Mathf.Sin(rad), -Mathf.Cos(rad), 0f);
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.right;

            Vector3 eye = player.transform.position + new Vector3(0f, 0f, -Plugin.EyeLift.Value);
            eye += forward * Plugin.EyeForward.Value;
            Look.Accumulate();
            Quaternion rot = Quaternion.LookRotation(forward, Vector3.back);
            rot = Quaternion.AngleAxis(Look.Yaw, Vector3.back) * rot;
            rot = Quaternion.AngleAxis(Look.Pitch, rot * Vector3.right) * rot;
            forward = rot * Vector3.forward;

            Camera cam = __instance.GetComponent<Camera>();
            if (cam == null)
                cam = __instance.GetComponentInChildren<Camera>();
            Transform rig = cam != null ? cam.transform : __instance.transform;
            if (cam != null)
            {
                cam.orthographic = false;
                cam.fieldOfView = Plugin.Fov.Value;
                cam.nearClipPlane = 0.05f;
            }
            rig.position = eye;
            rig.rotation = rot;
            Once("locked pos=" + eye + " aim=" + angle + " cam=" + (cam != null));
        }

        static void Once(string message)
        {
            if (Plugin.Reported)
                return;
            Plugin.Reported = true;
            if (Plugin.Log != null)
                Plugin.Log.LogInfo(message);
        }

        static Component Player()
        {
            if (playerType == null)
            {
                playerType = AccessTools.TypeByName("PlayerFarming");
                if (playerType == null)
                    return null;
                playerInstance = playerType.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                aimAngle = playerType.GetField("AimAngle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                cameraBone = playerType.GetField("CameraBone", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (playerInstance == null)
                return null;
            return playerInstance.GetValue(null) as Component;
        }
    }

    static class Look
    {
        public static float Yaw;
        public static float Pitch;
        static int frame = -1;

        public static void Accumulate()
        {
            if (Time.frameCount == frame)
                return;
            frame = Time.frameCount;
            Yaw += Input.GetAxis("Mouse X") * 4.5f;
            Pitch = Mathf.Clamp(Pitch - Input.GetAxis("Mouse Y") * 3.2f, -20f, 28f);
        }
    }
}
