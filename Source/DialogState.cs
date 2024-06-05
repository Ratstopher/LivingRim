using System;
using Verse;

namespace LivingRim
{
    public class DialogState : IExposable
    {
        public string LastUserMessage;
        public string LastModelResponse;

        public DialogState() { }

        public DialogState(string lastUserMessage, string lastModelResponse)
        {
            LastUserMessage = lastUserMessage;
            LastModelResponse = lastModelResponse;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref LastUserMessage, "LastUserMessage");
            Scribe_Values.Look(ref LastModelResponse, "LastModelResponse");
        }
    }
}
