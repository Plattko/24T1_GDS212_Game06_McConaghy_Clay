using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;
using System.Linq;

namespace Plattko
{
    public class NoteGenerator : MonoBehaviour
    {
        // Reference variables
        private AudioSource audioSource;
        private Lane[] lanes;
        public MidiFile midiFile;
        private string fileLocation;

        [Header("Lane Settings")]
        [SerializeField] private int leftLaneDivider = 65;

        [Header("Note Settings")]
        public float noteSpawnY;
        public double quarterNoteHeight = 1.1f;
        public double quarterNoteDurationInSeconds;
        public double noteFallSpeed;

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
            // Add the notes to a collection
            var notes = midiFile.GetNotes();

            // Get tempo and note fall speed (assuming 4/4 time)
            Tempo tempo = midiFile.GetTempoMap().GetTempoAtTime(new MetricTimeSpan(0, 0, 0));
            double beatsPerSecond = tempo.BeatsPerMinute / 60f;
            quarterNoteDurationInSeconds = 1f / beatsPerSecond;
            noteFallSpeed = beatsPerSecond * quarterNoteHeight;

            // Send each note to its corresponding lane
            foreach (Lane lane in lanes)
            {
                var laneNotes = notes.Where(note => note.NoteNumber == lane.laneNoteNumber).ToArray();
                lane.GetNoteData(laneNotes);
            }
        }

        public double GetAudioSourceTime()
        {
            return (double)audioSource.timeSamples / audioSource.clip.frequency;
        }
    }
}
