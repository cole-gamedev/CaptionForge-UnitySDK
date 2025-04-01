using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.IO;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace CaptionForge.Editor
{
    public class CaptionForge : EditorWindow
    {
        private const int WaveFormTextureWidth = 2048;
        private const int WaveFormTextureHeight = 512;
        private const float WaveFormLabelScale = 0.3f;
        
        private int _scaledWaveFormWidth;
        private int _scaledWaveFormHeight;
        
        private AudioClip _audioClipAsset;
        private Texture2D _audioClipTexture;
        private Texture2D _backgroundTexture;
        private bool _generatingTexture;
        
        private Color _backgroundColor;

        private bool _repaintContents;
        
        // Subtitle Sections
        private bool _createNewSection; // set true on mouse down, executed in mouse drag
        private const float DragCreationLength = 8;
        private float _createSectionX;
        private List<SubtitleSection> _subtitleSections;
        private SubtitleSection _selectedSubtitleSection;
        private SubtitleSection _checkSelectingSubtitle;
        
        // Sample Start Point
        private const float SampleHandleWidth = 10f;
        private const float SampleHandleHeight = 20f;
        private Rect _startPointHandleRect;
        private Rect _sampleLine;
        private float _sampleLineX;
        private readonly Color _sampleLineColor = Color.white;
        private bool _movingSampleLine;
        
        // AudioSample
        private Rect _waveFormSampleRect;
        
        // AudioClip Sample Position
        private float _lineX;
        private float _sampleCount;
        
        // Keyboard Inputs
        private Dictionary<KeyCode, Action> _keyDownBindings;
        
        // Button Content
        private GUIContent _playButtonContent;
        private GUIContent _pauseButtonContent;
        private GUIContent _stopButtonContent;
        private GUIContent _beginButtonContent;
        
        // Text Area Properties
        private float _textAreaHeight;
        
        // Save File
        private string _srtContents;
        private string _saveFilePath;
        
        [MenuItem("Tools/Caption Forge")]
        public static void ShowWindow()
        {
            GetWindow<CaptionForge>("Caption Forge");
        }

        private void OnEnable()
        {
            _playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            _playButtonContent.tooltip = "Start Playing";
            _stopButtonContent = EditorGUIUtility.IconContent("PreMatQuad");
            _stopButtonContent.tooltip = "Stop Playing";
            _beginButtonContent = EditorGUIUtility.IconContent("beginButton");
            _beginButtonContent.tooltip = "Move start to beginning";
            _pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            _pauseButtonContent.tooltip = "Pause Audio";
            
            _subtitleSections = new List<SubtitleSection>();
            
            _backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
            _backgroundTexture = GetFallbackTexture(WaveFormTextureWidth, WaveFormTextureHeight);
            
            _scaledWaveFormWidth = Mathf.CeilToInt(WaveFormTextureWidth * WaveFormLabelScale);
            _scaledWaveFormHeight = Mathf.CeilToInt(WaveFormTextureHeight * WaveFormLabelScale);
            
            _startPointHandleRect = new Rect(0, 0, SampleHandleWidth, SampleHandleHeight);
            _sampleLine = new Rect(0, 0, 1, _scaledWaveFormHeight);
            
            _keyDownBindings = new Dictionary<KeyCode, Action>
            { 
                {
                    KeyCode.Delete, () =>
                    {
                        if (_selectedSubtitleSection == null) return;
                        _subtitleSections.Remove(_selectedSubtitleSection);
                        _repaintContents = true;
                    }
                } 
            };
        }

        private void OnDisable()
        {
            if (_audioClipAsset == null) return;
            
            if (EditorAudioUtility.HasPreview(_audioClipAsset) && EditorAudioUtility.IsClipPlaying())
            {
                EditorAudioUtility.StopAllClips();
            }
        }

        private void OnGUI()
        {
            var e = Event.current;
            
            // Check for user input events
            CheckInput(e);
            
            EditorGUILayout.BeginVertical();
            {
                EditorGUI.BeginDisabledGroup(_generatingTexture);
                {
                    EditorGUI.BeginChangeCheck();
                    _audioClipAsset =
                        EditorGUILayout.ObjectField("Audio Clip", _audioClipAsset, typeof(AudioClip), false) as
                            AudioClip;
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (!_generatingTexture && _audioClipAsset)
                        {
                            Debug.Log("Waiting for clip to load...");
                            EditorCoroutineUtility.StartCoroutine(WaitForClipToLoad(_audioClipAsset), this);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(20f);

                GUILayout.Label(_audioClipTexture != null ? _audioClipTexture : _backgroundTexture,
                    GUILayout.Width(_scaledWaveFormWidth), GUILayout.Height(_scaledWaveFormHeight));

                if (e.type == EventType.Repaint)
                {
                    _waveFormSampleRect = GUILayoutUtility.GetLastRect();
                }

                // Draw Media Controls Section
                EditorGUILayout.BeginHorizontal(GUILayout.Width(_scaledWaveFormWidth));
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        
                        EditorGUI.BeginDisabledGroup(!(_sampleLineX > _waveFormSampleRect.xMin + 1));
                        {
                            if (GUILayout.Button(_beginButtonContent, GUILayout.Width(30), GUILayout.Height(30)))
                            {
                                _sampleLineX = _waveFormSampleRect.xMin;
                                _repaintContents = true;
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!_audioClipAsset);
                        {
                            if (EditorAudioUtility.IsClipPlaying())
                            {
                                if (GUILayout.Button(_stopButtonContent, GUILayout.Width(30), GUILayout.Height(30)))
                                {
                                    EditorAudioUtility.StopAllClips();
                                    _repaintContents = true;
                                }
                            }
                            else
                            {
                                if (GUILayout.Button(_playButtonContent, GUILayout.Width(30), GUILayout.Height(30)))
                                {
                                    EditorAudioUtility.StopAllClips();
                                    EditorAudioUtility.PlayClip(_audioClipAsset, GetStartSamplePosition());
                                    _repaintContents = true;
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(!_audioClipAsset || !EditorAudioUtility.IsClipPlaying());
                        {
                            if (GUILayout.Button(_pauseButtonContent, GUILayout.Width(30), GUILayout.Height(30)))
                            {
                                EditorAudioUtility.PauseClip();
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10f);

                if (e.type == EventType.Repaint)
                {
                    _textAreaHeight = position.height - GUILayoutUtility.GetLastRect().yMax - 50;
                }

                EditorGUI.BeginDisabledGroup(_selectedSubtitleSection == null);
                {
                    if (_selectedSubtitleSection != null)
                    {
                        _selectedSubtitleSection.Text = GUILayout.TextArea(_selectedSubtitleSection.Text, GUILayout.Width(_scaledWaveFormWidth), GUILayout.Height(_textAreaHeight));
                    }
                    else
                    {
                        GUILayout.TextArea("", GUILayout.Width(_scaledWaveFormWidth), GUILayout.Height(_textAreaHeight));    
                    }
                }
                EditorGUI.EndDisabledGroup();
                
                GUILayout.Space(5f);
                
                EditorGUILayout.BeginHorizontal(GUILayout.Width(_scaledWaveFormWidth));
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Export SRT", GUILayout.Width(130)))
                    {
                        // Generate SRT File
                        _srtContents = GenerateSrtContents();
                        if (String.IsNullOrEmpty(_srtContents) == false && _audioClipAsset)
                        {
                            _saveFilePath = EditorUtility.SaveFilePanel("Save Srt File", "", $"{_audioClipAsset.name}", "srt");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            // Draw current Sample Line
            if (_audioClipAsset != null && EditorAudioUtility.HasPreview(_audioClipAsset) && EditorAudioUtility.IsClipPlaying())
            {
                if (e.type == EventType.Repaint)
                {
                    if (_sampleCount > 0)
                    {
                        _lineX = Mathf.Lerp(_waveFormSampleRect.xMin, _waveFormSampleRect.xMax - _waveFormSampleRect.xMin,
                            EditorAudioUtility.GetClipSamplePosition() / _sampleCount);
                    }
                    else
                    {
                        _lineX = 0;
                    }

                    // Draw the line
                    var lineRect = new Rect(_lineX, _waveFormSampleRect.y, 1, _waveFormSampleRect.height);
                    EditorGUI.DrawRect(lineRect, _sampleLineColor);
                }

                // Force repaint to reflect changes instantly
                _repaintContents = true;
            }
            
            // Draw Start Sample Point Handle and Line
            _sampleLineX = Mathf.Clamp(_sampleLineX, _waveFormSampleRect.xMin, _waveFormSampleRect.xMax - _waveFormSampleRect.xMin);
            _startPointHandleRect.x = _sampleLineX - (SampleHandleWidth * 0.5f);
            _startPointHandleRect.y = _waveFormSampleRect.yMin - (SampleHandleHeight * 0.5f);

            _sampleLine.x = _sampleLineX;
            _sampleLine.y = _waveFormSampleRect.yMin;

            if (e.type == EventType.Repaint)
            {
                for (var i = 0; i < _subtitleSections.Count; i++)
                {
                    EditorGUI.DrawRect(_subtitleSections[i].SectionRect, _subtitleSections[i].SectionColor);
                }
                
                EditorGUI.DrawRect(_sampleLine, Color.white * 0.9f);
                EditorGUI.DrawRect(_startPointHandleRect, Color.grey);
            }

            switch (e.type)
            {
                case EventType.MouseDown:
                    // Check if user clicked on a subtitle section
                    for (var i = 0; i < _subtitleSections.Count; i++)
                    {
                        if (!_subtitleSections[i].SectionRect.Contains(e.mousePosition)) continue;
                        
                        _checkSelectingSubtitle = _subtitleSections[i];
                        break;
                    }
                    
                    if (_startPointHandleRect.Contains(e.mousePosition))
                    {
                        _movingSampleLine = true;
                        GUI.FocusControl(null);
                    }
                    else if(_waveFormSampleRect.Contains(e.mousePosition) && _checkSelectingSubtitle == null)
                    {
                        _createSectionX = e.mousePosition.x;
                        _createNewSection = true;
                    }

                    break;
                case EventType.MouseDrag:
                    if (_movingSampleLine)
                    {
                        _sampleLineX = Mathf.Clamp(e.mousePosition.x, _waveFormSampleRect.xMin, _waveFormSampleRect.xMax - _waveFormSampleRect.xMin);
                        _repaintContents = true;
                    }

                    if (_createNewSection)
                    {
                        if (e.mousePosition.x - _createSectionX > DragCreationLength)
                        {
                            _createNewSection = false;

                            CreateSubtitleSection(_createSectionX);
                        }
                    }
                    
                    if (_selectedSubtitleSection is { Resizing: true })
                    {
                        _selectedSubtitleSection.SectionRect.width = Mathf.Clamp(
                            e.mousePosition.x - _selectedSubtitleSection.SectionRect.xMin,
                            1,
                            _waveFormSampleRect.xMax - _waveFormSampleRect.xMin - _selectedSubtitleSection.SectionRect.xMin - 1);
                        
                        _repaintContents = true;
                    }
                    break;
                case EventType.MouseUp:
                    if (_movingSampleLine)
                    {
                        _movingSampleLine = false;
                    }

                    if (_selectedSubtitleSection is { Resizing: true })
                    {
                        _selectedSubtitleSection.IsResizing(false);
                    }

                    if (_checkSelectingSubtitle != null)
                    {
                        if (_checkSelectingSubtitle.SectionRect.Contains(e.mousePosition))
                        {
                            // Selected Subtitle Section has changed!
                            _selectedSubtitleSection?.SetSelected(false);
                            _selectedSubtitleSection = _checkSelectingSubtitle;
                            _selectedSubtitleSection.SetSelected(true);
                            _repaintContents = true;
                        }
                        _checkSelectingSubtitle = null;
                    }

                    _createNewSection = false;
                    break;
            }
            
            // Check if saving file
            if (String.IsNullOrEmpty(_saveFilePath) == false)
            {
                File.WriteAllText(_saveFilePath, _srtContents);
                AssetDatabase.Refresh();
                AssetHighlighter.ShowFile(_saveFilePath);
                
                Debug.Log($"File Exported! ({_saveFilePath})");
                _saveFilePath = null;
            }
            
            // Repaint window contents if it needs to
            if (!_repaintContents) return;
            _repaintContents = false;
            Repaint();
        }

        private void CheckInput(Event e)
        {
            if (e.type == EventType.KeyDown)
            {
                if (_keyDownBindings.ContainsKey(e.keyCode))
                {
                    _keyDownBindings[e.keyCode]?.Invoke();                    
                }
            }
        }

        private string GenerateSrtContents()
        {
            var content = "";
            var subtitleIndex = 1;

            SortSubtitles(ref _subtitleSections);

            var clipDuration = EditorAudioUtility.GetDuration(_audioClipAsset) / 1000;

            foreach (var subtitleSection in _subtitleSections)
            {
                content += $"{subtitleIndex}\n";
                
                double subtitleStartTime = Mathf.InverseLerp(_waveFormSampleRect.xMin, _waveFormSampleRect.xMax - _waveFormSampleRect.xMin, subtitleSection.SectionRect.xMin);
                subtitleStartTime *= clipDuration;
                double subtitleEndTime = Mathf.InverseLerp(_waveFormSampleRect.xMin, _waveFormSampleRect.xMax - _waveFormSampleRect.xMin, subtitleSection.SectionRect.xMax);
                subtitleEndTime *= clipDuration;
                
                content += $"{ConvertToSrtTimestamp(subtitleStartTime)} --> {ConvertToSrtTimestamp(subtitleEndTime)}\n";

                if (subtitleIndex < _subtitleSections.Count)
                {
                    content += $"{subtitleSection.Text}\n\n";

                    subtitleIndex += 1;
                }
                else
                {
                    content += $"{subtitleSection.Text}";
                }
            }
            
            return content;
        }

        private static void SortSubtitles(ref List<SubtitleSection> subtitleSections)
        {
            var n = subtitleSections.Count;
            
            for (var i = 0; i < n - 1; i++)
            {
                for (var j = 0; j < n - i - 1; j++)
                {
                    if (subtitleSections[j].SectionRect.xMin > subtitleSections[j + 1].SectionRect.xMin)
                    {
                        (subtitleSections[j], subtitleSections[j + 1]) = (subtitleSections[j + 1], subtitleSections[j]);
                    }
                }
            }
        }
        
        private static string ConvertToSrtTimestamp(double timeInSeconds)
        {
            var hours = (int)(timeInSeconds / 3600);
            var minutes = (int)((timeInSeconds % 3600) / 60);
            var seconds = (int)(timeInSeconds % 60);
            var milliseconds = (int)((timeInSeconds - Math.Floor(timeInSeconds)) * 1000);

            return $"{hours:D2}:{minutes:D2}:{seconds:D2},{milliseconds:D3}";
        }
        
        private void CreateSubtitleSection(float xPos)
        {
            _selectedSubtitleSection?.SetSelected(false);

            Rect sectionRect = new Rect(new Vector2(xPos, _waveFormSampleRect.yMin), new Vector2(1, _waveFormSampleRect.height));
            SubtitleSection newSubtitle = new SubtitleSection(sectionRect, "", new Color(0.133f, 0.57f, 0.85f, 0.3f));
            _subtitleSections.Add(newSubtitle);
            _selectedSubtitleSection = newSubtitle;
            _selectedSubtitleSection.IsResizing(true);
            _selectedSubtitleSection.SetSelected(true);
            
            _repaintContents = true;
        }
        
        private int GetStartSamplePosition()
        {
            var startSamplePosition = 0;
            var startPercent = (_sampleLineX - _waveFormSampleRect.xMin) / (_waveFormSampleRect.xMax - (_waveFormSampleRect.xMin * 2));
            startSamplePosition = Mathf.RoundToInt(_sampleCount * startPercent);
            
            return startSamplePosition;
        }
        
        /// <summary>
        /// Waits for the <param name="audioClip"></param> to be loaded before generating a texture for it
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        private IEnumerator WaitForClipToLoad(AudioClip audioClip)
        {
            _generatingTexture = true;

            if (audioClip.LoadAudioData())
            {

                while (audioClip.loadState == AudioDataLoadState.Loading)
                {
                    yield return null;
                }

                if (audioClip.loadState == AudioDataLoadState.Loaded)
                {
                    yield return null;

                    _sampleCount = EditorAudioUtility.GetSampleCount(audioClip);

                    _audioClipTexture = GetWaveformTexture(audioClip, WaveFormTextureWidth, WaveFormTextureHeight,
                        Color.cyan);

                    if (_audioClipTexture != null)
                    {
                        _audioClipTexture =
                            ResizeTexture(_audioClipTexture, _scaledWaveFormWidth, _scaledWaveFormHeight);
                    }

                    Repaint();
                }
            }
            else
            {
                Debug.LogError($"Failed to load audioclip ({audioClip.name})");
            }

            yield return null;
            _generatingTexture = false;
        }   
        
        /// <summary>
        /// Generates texture for audioclip
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="waveformColor"></param>
        /// <returns></returns>
        private Texture2D GetWaveformTexture(AudioClip audioClip, int width, int height, Color waveformColor)
        {
            if (audioClip == null) return null;

            // Create a blank texture
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color backgroundColor = _backgroundColor * 0.7f; // Change this if needed
            Color[] pixels = new Color[width * height];

            // Fill background
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = backgroundColor;

            texture.SetPixels(pixels);
            texture.Apply();

            // Get audio samples
            float[] samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            int packSize = samples.Length / width; // How many samples per column

            // Draw waveform
            for (int x = 0; x < width; x++)
            {
                int startSample = x * packSize;
                float maxSample = 0f;

                // Find the peak sample in the given range
                for (int i = 0; i < packSize; i++)
                {
                    int sampleIndex = startSample + i;
                    if (sampleIndex < samples.Length)
                    {
                        maxSample = Mathf.Max(maxSample, Mathf.Abs(samples[sampleIndex]));
                    }
                }

                if (maxSample < 0.01f)
                {
                    maxSample = 0.01f;
                }
                
                // Convert sample value to pixel height
                int pixelHeight = Mathf.RoundToInt(maxSample * (height / 2f));
                int midY = height / 2;

                // Draw vertical line for the waveform
                for (int y = midY - pixelHeight; y <= midY + pixelHeight; y++)
                {
                    texture.SetPixel(x, Mathf.Clamp(y, 0, height - 1), waveformColor);
                }
            }

            texture.Apply();
            return texture;
        }

        private Texture2D GetFallbackTexture(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var backgroundColor = _backgroundColor * 0.7f;
            var pixels = new Color[width * height];

            // Fill background
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }
        
        /// <summary>
        /// Resizes texture
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            var rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;

            var result = new Texture2D(width, height, source.format, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }
        
    }
}