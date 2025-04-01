// Reflection class for methods provided via UnityEditor.AudioUtil for audio controls in Editor

using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace CaptionForge
{
    public static class EditorAudioUtility
    {
        private static readonly MethodInfo PlayClipMethod;
        private static readonly MethodInfo PauseClipMethod;
        private static readonly MethodInfo ResumeClipMethod;
        private static readonly MethodInfo LoopClipMethod;
        private static readonly MethodInfo IsClipPlayingMethod;
        private static readonly MethodInfo StopAllClipsMethod;
        private static readonly MethodInfo GetClipPositionMethod;
        private static readonly MethodInfo GetClipSamplePositionMethod;
        private static readonly MethodInfo SetClipSamplePositionMethod;
        private static readonly MethodInfo GetSampleCountMethod;
        private static readonly MethodInfo GetChannelCountMethod;
        private static readonly MethodInfo GetBitRateMethod;
        private static readonly MethodInfo GetBitsPerSampleMethod;
        private static readonly MethodInfo GetFrequencyMethod;
        private static readonly MethodInfo GetSoundSizeMethod;
        private static readonly MethodInfo HasPreviewMethod;
        private static readonly MethodInfo GetDurationMethod;
        private static readonly MethodInfo GetMusicChannelCountMethod;
        
        static EditorAudioUtility()
        {
            var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            
            // Get method references
            PlayClipMethod = audioUtilType.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
            PauseClipMethod = audioUtilType.GetMethod("PausePreviewClip", BindingFlags.Static | BindingFlags.Public);
            ResumeClipMethod = audioUtilType.GetMethod("ResumePreviewClip", BindingFlags.Static | BindingFlags.Public);
            LoopClipMethod = audioUtilType.GetMethod("LoopPreviewClip", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(bool) }, null);
            IsClipPlayingMethod = audioUtilType.GetMethod("IsPreviewClipPlaying", BindingFlags.Static | BindingFlags.Public);
            StopAllClipsMethod = audioUtilType.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public);
            GetClipPositionMethod = audioUtilType.GetMethod("GetPreviewClipPosition", BindingFlags.Static | BindingFlags.Public);
            GetClipSamplePositionMethod = audioUtilType.GetMethod("GetPreviewClipSamplePosition", BindingFlags.Static | BindingFlags.Public);
            SetClipSamplePositionMethod = audioUtilType.GetMethod("SetPreviewClipSamplePosition", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip), typeof(int) }, null);
            GetSampleCountMethod = audioUtilType.GetMethod("GetSampleCount", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            GetChannelCountMethod = audioUtilType.GetMethod("GetChannelCount", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            GetBitRateMethod = audioUtilType.GetMethod("GetBitRate", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            GetBitsPerSampleMethod = audioUtilType.GetMethod("GetBitsPerSample", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            GetFrequencyMethod = audioUtilType.GetMethod("GetFrequency", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            GetSoundSizeMethod = audioUtilType.GetMethod("GetSoundSize", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            HasPreviewMethod = audioUtilType.GetMethod("HasPreview", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            GetDurationMethod = audioUtilType.GetMethod("GetDuration", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            GetMusicChannelCountMethod = audioUtilType.GetMethod("GetMusicChannelCount", BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            
            Debug.Log("EditorAudioUtility: Initialized!");
        }
        
        /// <summary>
        /// Sets the "Preview" Clip and plays it in Editor
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="startSample"></param>
        /// <param name="loop"></param>
        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            PlayClipMethod?.Invoke(null, new object[] { clip, startSample, loop });
        }

        /// <summary>
        /// Pauses the current playing "Preview" Clip
        /// </summary>
        public static void PauseClip()
        {
            PauseClipMethod?.Invoke(null, null);
        }
        
        /// <summary>
        /// Resumes playing the current "Preview" Clip
        /// </summary>
        public static void ResumeClip()
        {
            ResumeClipMethod?.Invoke(null, null);
        }

        /// <summary>
        /// Sets if the preview clip should be playing on loop
        /// <br/>
        /// This does not play the clip
        /// </summary>
        /// <param name="on"></param>
        public static void LoopClip(bool on)
        {
            LoopClipMethod?.Invoke(null, new object[] { on });
        }

        /// <summary>
        /// Checks if there is a preview clip currently playing
        /// </summary>
        /// <returns></returns>
        public static bool IsClipPlaying()
        {
            return (bool)IsClipPlayingMethod?.Invoke(null, null)!;
        }
        
        /// <summary>
        /// Stops any current playing "Preview" Clips
        /// </summary>
        public static void StopAllClips()
        {
            StopAllClipsMethod?.Invoke(null, null);
        }

        /// <summary>
        /// Returns the current Position of the current "Preview" Clip
        /// </summary>
        /// <returns></returns>
        public static float GetClipPosition()
        {
            return (float)GetClipPositionMethod?.Invoke(null, null)!;
        }

        /// <summary>
        /// Returns the current Clip Sample Position of the current "Preview" Clip
        /// </summary>
        /// <returns></returns>
        public static int GetClipSamplePosition()
        {
            return (int)GetClipSamplePositionMethod?.Invoke(null, null)!;
        }

        /// <summary>
        /// Sets the Clip Sample Position for <param name="clip"></param> by <param name="iSamplePosition"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="iSamplePosition"></param>
        public static void SetClipSamplePosition(AudioClip clip, int iSamplePosition)
        {
            SetClipSamplePositionMethod?.Invoke(null, new object[] { clip, iSamplePosition });
        }

        /// <summary>
        /// Returns the Sample Count for <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static int GetSampleCount(AudioClip clip)
        {
            return (int)GetSampleCountMethod?.Invoke(null, new object[] { clip })!;
        }

        /// <summary>
        /// Returns the Channel Count for <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static int GetChannelCount(AudioClip clip)
        {
            return (int)GetChannelCountMethod?.Invoke(null, new object[] { clip })!;
        }
        
        /// <summary>
        /// Returns the BitRate for <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static int GetBitRate(AudioClip clip)
        {
            return (int)GetBitRateMethod?.Invoke(null, new object[] { clip })!;
        }
        
        /// <summary>
        /// Returns the Bits Per Sample for <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static int GetBitsPerSample(AudioClip clip)
        {
            return (int)GetBitsPerSampleMethod?.Invoke(null, new object[] { clip })!;
        }

        /// <summary>
        /// Returns the Frequency for <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static int GetFrequency(AudioClip clip)
        {
            return (int)GetFrequencyMethod?.Invoke(null, new object[] { clip })!;
        }

        /// <summary>
        /// Returns the Sound Size for <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static int GetSoundSize(AudioClip clip)
        {
            return (int)GetSoundSizeMethod?.Invoke(null, new object[] { clip })!;
        }

        /// <summary>
        /// Returns if <param name="clip"></param> is set and loaded as the current "Preview"
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static bool HasPreview(AudioClip clip)
        {
            return (bool)HasPreviewMethod?.Invoke(null, new object[] { clip })!;
        }
        
        /// <summary>
        /// Returns the Duration of <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static double GetDuration(AudioClip clip)
        {
            return (double)GetDurationMethod?.Invoke(null, new object[] { clip })!;
        }

        /// <summary>
        /// Returns the Music Channel Count for <param name="clip"></param>
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static int GetMusicChannelCount(AudioClip clip)
        {
            return (int)GetMusicChannelCountMethod?.Invoke(null, new object[] { clip })!;
        }
    }
}