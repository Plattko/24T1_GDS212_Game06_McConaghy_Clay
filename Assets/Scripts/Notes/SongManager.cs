using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;

namespace Plattko
{
    public class SongManager : MonoBehaviour
    {
        private AudioSource audioSource;
        
        private float songDelay;

        private string fileLocation;
        public float noteTime;
        public float noteSpawnY;
        public float noteDespawnY;

        public MidiFile midiFile;

        private Lane[] lanes;
        
        void Start()
        {
            if (Application.streamingAssetsPath.StartsWith("http://") || Application.streamingAssetsPath.StartsWith("https://"))
            {
                StartCoroutine(ReadFromWebsite());
            }
            else
            {
                ReadFromFile();
            }
        }

        void Update()
        {
        
        }

        private IEnumerator ReadFromWebsite()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileLocation))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    byte[] results = www.downloadHandler.data;

                    using (var stream = new MemoryStream(results))
                    {
                        midiFile = MidiFile.Read(stream);
                        GetDataFromMidi();
                    }
                }
            }
        }

        private void ReadFromFile()
        {
            midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);
            GetDataFromMidi();
        }

        private void GetDataFromMidi()
        {
            // Add the notes to an array
            var notes = midiFile.GetNotes();
            var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
            notes.CopyTo(array, 0);

            foreach (Lane lane in lanes)
            {
                lane.GetNoteData(array);
            }

            Invoke(nameof(StartSong), songDelay);
        }

        public double GetAudioSourceTime()
        {
            return (double)audioSource.timeSamples / audioSource.clip.frequency;
        }

        private void StartSong()
        {
            // Play audio source
        }
    }
}
