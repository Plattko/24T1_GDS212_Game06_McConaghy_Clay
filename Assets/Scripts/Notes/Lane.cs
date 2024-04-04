using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Interaction;

namespace Plattko
{
    public class Lane : MonoBehaviour
    {
        private NoteGenerator noteGenerator;
        [SerializeField] private GameObject notePrefab;

        [Header("Lane Settings")]
        public int laneNoteNumber;
        [SerializeField] private bool isBlackNote;

        // Note spawning
        private List<double> noteTimeStamps = new List<double>();
        private List<double> noteDurations = new List<double>();
        private int spawnIndex = 0;

        private float noteWidth;
        private Color noteColour;

        private float songTimer = 0f;

        void Start()
        {
            noteGenerator = GameObject.FindGameObjectWithTag("NoteGenerator").GetComponent<NoteGenerator>();
            
            // Set note width
            if (isBlackNote)
            {
                noteWidth = noteGenerator.blackNoteWidth;
            }
            else
            {
                noteWidth = noteGenerator.whiteNoteWidth;
            }
            
            // Set note colour
            if (laneNoteNumber < noteGenerator.leftLaneDivider)
            {
                if (isBlackNote)
                {
                    noteColour = new Color32(19, 101, 211, 255);
                }
                else
                {
                    noteColour = new Color32(104, 157, 207, 255);
                }
            }
            else if (laneNoteNumber >= noteGenerator.leftLaneDivider)
            {
                if (isBlackNote)
                {
                    noteColour = new Color32(51, 171, 0, 255);
                }
                else
                {
                    noteColour = new Color32(118, 230, 0, 255);
                }
            }
        }
        
        void Update()
        {
            songTimer += Time.deltaTime;
            
            if (spawnIndex < noteTimeStamps.Count)
            {
                if (songTimer >= noteTimeStamps[spawnIndex])
                {
                    float noteHeight = (float)(noteDurations[spawnIndex] * noteGenerator.quarterNoteHeight);
                    GameObject note = Instantiate(notePrefab, new Vector2(transform.position.x, noteGenerator.noteSpawnY + (noteHeight / 2)), Quaternion.identity);
                    //Debug.Log("Instantiated Note: " + note);
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
