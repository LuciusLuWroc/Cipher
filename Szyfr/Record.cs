using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szyfr
{
    /// <summary>
    /// Input data 
    /// </summary>
    public class Record
    {        

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cipherSample">Próbka szyfru. Cipher sample</param>
        /// <param name="matchedCount">Ilość poprawnych cyfr w próbce szyfru. correct digits count in CipherSample</param>
        /// <param name="isOnRightPosition">czy poprawne cyfry znajdują się na prawidłowej pozycji. Is properly position?</param>
        /// <param name="name">Nazwa rekordu w postaci liczby całkowitej. Record name</param>
        public Record(string cipherSample, int matchedCount, bool isOnRightPosition, int name)
        {
            StrToList(cipherSample);
            MatchedCount = matchedCount;
            IsOnRightPosition = isOnRightPosition;
            Name = name;            
        }

        /// <summary>
        /// Próbka szyfru.
        /// Cipher sample.
        /// </summary>
        public List<int?> CipherSample { get;  set; }

        /// <summary>        
        /// Ilość poprawnych cyfr w próbce szyfru.
        /// Correct digits in CipherSample count.
        /// </summary>
        public int MatchedCount { get; private set; }

        /// <summary>        
        /// Flaga wskazująca czy poprawne cyfry są na właściwych pozycjach
        /// Boolean value pointing whether correct digits in CipherSample are on correct places
        /// </summary>        
        public bool IsOnRightPosition { get; private set; }

        /// <summary>
        /// Nazwa rekordu w postaci liczby całkowitej
        /// Record Name
        /// </summary>
        public int Name { get; private set; }

        /// <summary>
        /// Zwraca nazwę rekordu w postaci stringu
        /// Returns Record Name as string
        /// </summary>
        /// <returns></returns>
        public string ToString()
        {
            return Name.ToString();
        }

        /// <summary>
        /// Zwraca ilość pozycji w liście CipherSample różnych od null
        /// Returns nonull digits count in CipherSample.
        /// </summary>
        /// <returns>int</returns>
        public int NotNullCount()
        {
            int l = 0;
            foreach (var v in CipherSample)
                if (v != null) l++;
            return l;
        }
        
        /// <summary>
        /// Konwertuje próbkę szyfru w postaci stringu do listy liczb całkowitych nullable
        /// Converts cipherSample as string (vector) to List of nullable integers.
        /// </summary>
        /// <param name="vector">string z 3 cyframi. 3 digits as string </param>
        private void StrToList(string vector)
        {
            if (vector.Length != 3) throw new Exception("Incorrect vector length");
            CipherSample = new List<int?>(3);            
            foreach( char c in vector )
            {
                if(c >= 48 && c <= 57)
                {
                    CipherSample.Add(int.Parse( c.ToString() ) );                    
                }
            }
            if (CipherSample.Count < 3) throw new Exception("Cipher Sample counts <3 digits!");
        }

        /// <summary>
        /// Usuwa w slotach niepoprawną cyfrę i ustawia wartość slotu na null
        /// Replaces wrongNr to null value in CipherSample
        /// </summary>
        /// <param name="wrongNr">Niepoprawna cyfra w szyfrze. Incorrect digit</param>
        public void UpdateBadNr(int? wrongNr)
        {            
            for(int i = 0; i < CipherSample.Count; i++)
            {
                if (CipherSample[i] == wrongNr) CipherSample[i] = null;
            }
        }
         
        /// <summary>
        /// Zwraca string reprezentujący CipherSample (Próbkę Szyfru)
        /// Returns string representing CipherSample
        /// </summary>
        /// <returns>string</returns>
        public string GetCipherSampleString()
        {
            string s = "";
            foreach(var ss in CipherSample)
            {
                if (ss == null) s += "_";
                else s += ss.ToString();
            }
            return s;
        }
    }
}
