using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Speech.Synthesis;



namespace Eyeplayer
{
    class TextToSpeech
    {
        SpeechSynthesizer reader;
        public bool isSpeech = false;
        public void Say(string str)
        {
            if (str == null || str == "") return;
            isSpeech = true;
            reader = new SpeechSynthesizer();
            reader.Rate = -3;
            reader.Volume = 100;
            reader.SpeakAsync(str);
            reader.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(SayCompleted);
            
        }
        void SayCompleted(object sender, SpeakCompletedEventArgs e)
        {
            isSpeech = false;
            reader.Dispose();
        }
        public void stop()
        {
            if (reader != null && isSpeech)
            {
                reader.SpeakAsyncCancelAll();
                isSpeech = false;
                reader.Dispose();
            }
        }

    }
}
