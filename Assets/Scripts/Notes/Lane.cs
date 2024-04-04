using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Interaction;

namespace Plattko
{
    public class Lane : MonoBehaviour
    {
        private NoteGenerator noteGenerator;
        private GameObject notePrefab;
        public int laneNoteNumber;

        // Note spawning
        private List<double> noteTimeStamps = new List<double>();
        private List<double> noteDurations = new List<double>();
        private int spawnIndex = 0;

        private float noteWidth;
        private Color noteColour;

        void Start()
        {
            
        }
        
        void Update()
        {
            if (spawnIndex < noteTimeStamps.Count)
            {
                if (noteGenerator.GetAudioSourceTime() >= noteTimeStamps[spawnIndex])
                {
                    float noteHeight = (float)(noteDurations[spawnIndex] * noteGenerator.quarterNoteHeight);
                    GameObject note = Instantiate(notePrefab, new Vector2(0, noteGenerator.noteSpawnY + (noteHeight / 2)), Quaternion.identity);
                    note.GetComponent<Note>().NoteSetup(noteWidth, noteHeight, noteColour, (float)noteGenerator.noteFallSpeed);

                    spawnIndex++;
                }
            }
        }

        public void GetNoteData(Melanchall.DryWetMidi.Interaction.Note[] notes)
        {
            foreach (var note in notes)
            {
                // Add note timestamp
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, noteGenerator.midiFile.GetTempoMap());
                noteTimeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + metricTimeSpan.Milliseconds / 1000f);

                var metricDuration = TimeConverter.ConvertTo<MetricTimeSpan>(note.Length, noteGenerator.midiFile.GetTempoMap());
                //noteDurations.Add((double)metricDuration.Minutes * 60f + metricDuration.Seconds + metricDuration.Milliseconds / 1000f);
                double durationInSeconds = metricDuration.Minutes * 60f + metricDuration.Seconds + metricDuration.Milliseconds / 1000f;
                noteDurations.Add(durationInSeconds / noteGenerator.quarterNoteDurationInSeconds);
            }
        }
    }
}
