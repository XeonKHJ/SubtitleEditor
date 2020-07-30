using System;
using System.Collections.Generic;

namespace SubtitleEditor.Subtitles
{
    public class Subtitle
    {
        public List<Dialogue> Dialogues { set; get; } = new List<Dialogue>();

        public void AddDialogue(Dialogue dialogue)
        {
            Dialogues.Add(dialogue);
            DialogueAdded?.Invoke(this, dialogue);
        }

        public void DeleteDialogue(Dialogue dialogue)
        {
            Dialogues.Remove(dialogue);
        }


        public event EventHandler<Dialogue> DialogueAdded;
    }
}
