using System;
using System.Collections.Generic;

namespace SubtitleEditor.Subtitles
{
    public class Subtitle
    {
        private List<Dialogue> _dialogues { set; get; } = new List<Dialogue>();

        public IReadOnlyList<Dialogue> Dialogues
        {
            get
            {
                return _dialogues;
            }
        }

        public void AddDialogue(Dialogue dialogue)
        {
            _dialogues.Add(dialogue);
            DialogueAdded?.Invoke(this, dialogue);
        }

        public void DeleteDialogue(Dialogue dialogue)
        {
            _dialogues.Remove(dialogue);
        }


        public event EventHandler<Dialogue> DialogueAdded;
    }
}
