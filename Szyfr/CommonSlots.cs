using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szyfr
{
    /// <summary>
    /// Dictionary of potentially correct digits, and methods
    /// </summary>
    public class CommonSlots
    {
        /// <summary>
        /// Dynamical potentially correct digits in specified positions (slots) dictionary,
        /// where key is slot nr, value is list of potentially correct digits.
        /// </summary>
        private Dictionary<int, List<int>> cs = new Dictionary<int, List<int>>();
                
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="slotCount">Domyślna ilość slotów. Default slots count</param>
        public CommonSlots(int slotsCount = 3)
        {
            SlotCount = slotsCount;
            if (slotsCount < 3)
                SlotCount = 3;
            
            for (int i = 1 ; i <= SlotCount; i++)
            {
                cs.Add(i, InitList());
            }
        }

        /// <summary>
        /// Flaga wskazująca czy Szyfr już został rozwiązany
        /// Flag pointing whether Cipher is solved
        /// </summary>
        public bool IsSolved
        {
            get
            {
                int licz = 0;
                foreach(var v in cs)
                {
                    if (v.Value.Count == 1) licz++;
                }
                if (licz > 2)
                {
                    solved = "";
                    foreach(var v in cs)
                    {
                        string s = v.Value[0].ToString();
                        solved += s;
                    }
                    
                    return true;
                }
                return false;
            }
        }

        string solved = "So pity. Not solved yet.";
        public string SolvedString()
        {
            return solved;
        }

        /// <summary>
        /// Zwraca listę liczb całkowitych zainicjowaną kolejnymi liczbami od 0 do 9.
        /// Returns list of integers list from 0 to 9.
        /// </summary>
        /// <returns>List &lt;int&gt;</returns>
        List<int> InitList()
        {
            List<int> l =  new List<int>();
            for (int i = 0; i < 10; i++)
                l.Add(i);
            return l;
        }

        /// <summary>
        /// Ilość slotów. Domyślnie 3
        /// Slots count, Default 3.
        /// </summary>
        public int SlotCount { get; set; }

        /// <summary>
        /// Usuwa wrongNrs z wszystkich list
        /// Removing wrongNrs from lists in all slots
        /// </summary>
        /// <param name="wrongNrs">Lista niepoprawnych cyfr</param>
        public void UpdateAllSlots(List<int> wrongNrs )
        {
            if (wrongNrs == null || wrongNrs.Count == 0) return;
            for (int i = 0; i < wrongNrs.Count; i++)
            {
                int v = wrongNrs[i];
                RemoveNrFromAllSlotsInCorrectInSlots(v);
            }
        }

        /// <summary>
        /// Usuwa wrongNrs z list znajdującej się w slocie slotNr
        /// Removing wrongNrs from list in slotNr
        /// </summary>
        /// <param name="wrongNrs">Lista niepoprawnych cyfr. Incorrect digits list</param>
        /// <param name="slotNr">Numer slotu. Slot nr, key of dictionary</param>
        public void UpdateOneSlot(List<int> wrongNrs, int slotNr)
        {
            if (wrongNrs == null || wrongNrs.Count == 0) return;
            for (int i = 0; i < wrongNrs.Count; i++)
            {
                int v = wrongNrs[i];
                RemoveFromSlot(slotNr, v);
            }
        }


        /// <summary>
        /// Usuwa niepoprawną cyfrę z list potencjalnych cyfr we wszystkich 3 slotach
        /// Removing incorrect digit from all lists of the dictionary.
        /// </summary>
        /// <param name="wrongNr">Niepoprawna cyfra w szyfrze. Incorrect digit</param>
        void RemoveNrFromAllSlotsInCorrectInSlots(int wrongNr)
        {
            for (int i = 1; i <= SlotCount; i++)
            {
                RemoveFromSlot(i, wrongNr);
            }
        }

        /// <summary>
        /// Usuwa z listy potencjalnych liczb dla slotu slotNr, liczbę toRemove
        /// Remove toRemove digit from list of key slotNr of the dictionary.
        /// </summary>
        /// <param name="slotNr">Numer slotu z przedziału [1..3]</param>
        /// <param name="toRemove">liczba do usunięcia z listy potencjalnych liczb dla slotu</param>
        void RemoveFromSlot(int slotNr, int toRemove)
        {
            List<int> l = cs[slotNr];
            if (!l.Contains(toRemove)) return;
            int idx = -1;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i] == toRemove)
                {
                    idx = i;
                    break;
                }
            }
            if (idx > -1)
                l.RemoveAt(idx);
            cs[slotNr] = l;
        }

        /// <summary>
        /// Zwraca listę potencjalnych liczb dla slotu o numerze slotNr
        /// Returns list Potentially Correct Digits from the Dictionary
        /// </summary>
        /// <param name="slotNr">Numer slotu z przedziału [1..3]. Slot nr</param>
        /// <returns>List&lt;int&gt;</returns>
        public List<int> GetListForSlotNr(int slotNr)
        {
            return cs[slotNr];
        }

        /// <summary>
        /// Odświeża dane po znalezieniu poprawnej liczby na poprawnej pozycji
        /// Ustawia w poprawnym slocie listę z jedną pozycją
        /// Usuwa z innych slotów tę cyfrę
        /// Sets correct slot with oneelement list, In other slot's lists removes the digit.
        /// </summary>
        /// <param name="correctNr">Poprawna cyfra. correct digit</param>
        /// <param name="correctSlotNr">Poprawna pozycja. Correct Slot/slot</param>
        public void UpdateFoundCorrectNr(int correctNr, int correctSlotNr)
        {
            List<int> l = new List<int>();
            l.Add(correctNr);
            cs[correctSlotNr] = l;
            for(int i = 1; i < cs.Count; i++)
            {
                if(i != correctSlotNr)
                {
                    UpdateOneSlot(l, i); // Usuwamy z innych slotów znalezioną już cyfrę
                }
            }
        }

        /// <summary>
        /// Zwraca stan slotów jako string
        /// Returns state of slots as string
        /// </summary>
        /// <returns>string</returns>
        public string GetState()
        {
            List<int> a1 = cs[1];
            List<int> a2 = cs[2];
            List<int> a3 = cs[3];
            string s = "CommonSlots 1= ";
            
            string lf = "\r\n";
            for (int i = 0; i < a1.Count; i++)
            {
                s += a1[i];
            }
            s += lf + "\t\t\t\t       2= ";
            for (int i = 0; i < a2.Count; i++)
            {
                s += a2[i];
            }
            s += lf + "\t\t\t\t       3= ";
            for (int i = 0; i < a3.Count; i++)
            {
                s += a3[i];
            }
            return s;

        }

        /// <summary>
        /// Returns last unsolved slot number, or zero if there is more unsolved slots
        /// </summary>
        public int NotCorrectSlotNr { get; private set; }

        /// <summary>
        /// Zwraca liczbę cyfr, które są poprawne i mają poprawną pozycję
        /// </summary>
        /// <returns>int</returns>
        public int FoundedCount()
        {
            int licz = 0;
            int slot = 0;
            for (int i = 1; i <= SlotCount; i++)
            {
                if (GetListForSlotNr(i).Count == 1) licz++;
                else slot = i;
            }
            if (licz == 2) NotCorrectSlotNr = slot;
            return licz;
        }
    }
}
