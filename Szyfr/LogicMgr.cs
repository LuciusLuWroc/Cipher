using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szyfr
{
    /// <summary>
    /// Algo logic manager
    /// </summary>
    public class LogicMgr
    {
        #region fields
        
        /// <summary>        
        /// Tymczasowy, dynamiczny Slownik, gdzie kluczem jest numer slotu, 
        /// a wartością lista potencjalnych cyfr dla danego slotu
        /// Dictionary of potentially correct digits in specified positions (slots),
        /// where key is slot nr, and value is list of potentially corect digits
        /// </summary>
        CommonSlots cs = new CommonSlots();

        /// <summary>
        /// wszystkie Recordy z właściwościami 
        /// List of all Records
        /// </summary>
        List<Record> list =  new List<Record>();
        
        /// <summary>
        /// Lista cyfr, które na pewno znajdują się w szyfrze
        /// dynamical list of correct digits
        /// </summary>
        List<int> correct = new List<int>(3);
        
        /// <summary>
        /// Pole przechowuje komunikaty błędów
        /// Errors
        /// </summary>
        string ERR = "";

        /// <summary>
        /// log Algorytmu
        /// Algorithm log
        /// </summary>
        string AlgoLog = "";

        #endregion fields

        #region ctors
        /*/// <summary>
        /// ctor
        /// </summary>
        public LogicMgr()
        {            
        }*/

        /// <summary>
        /// ctor Wczytuje zestaw danych z pliku, parsuje i ustawia dane
        /// Format danych w pliku w wierszu: 682,1,1
        /// Reads data from file
        /// Data format in file: 682,1,1 (CipherSample, Matched digits count, Is On Correct Position 1 = true)
        /// </summary>
        /// <param name="fileName">ścieżka do pliku z danymi wejściowymi kwizu. Fully qualified filename</param>
        public LogicMgr(string fileName)
        {
            string txt = "";            
            if (File.Exists(fileName)) txt = File.ReadAllText(fileName);
            else throw new Exception("There is no such file in the localization!");
            if (string.IsNullOrWhiteSpace(txt)) return;
            Parse(txt);            
            MainAlgo();
        }
        #endregion ctors

        #region Metody i właściwości obsługi danych wejściowych

        /// <summary>
        /// Parser
        /// </summary>
        /// <param name="input">data as text</param>
        void Parse(string input)
        {
            int iLicz = 0;
            string[] lines = input.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach(var line in lines)
            {
                string[] elm = line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (elm == null || elm.Length < 3) throw new Exception("Niepoprawny plik z danymi!");
                string vect = elm[0];
                string count = elm[1];
                string sloty = elm[2];
                bool b = false;
                if(sloty == "1") b = true;
                Record r = new Record(vect, int.Parse(count), b, iLicz++);
                this.Add(r);
            }
            Count = list.Count;
        }
                        
        /// <summary>
        /// Wstawia Record do list
        /// Adds Record to list
        /// </summary>
        /// <param name="vector">Obiekt z danymi - Próbka szyfru, ilość poprawnych i flaga czy na dobrym miejscu. Record </param>
        public void Add(Record vector)
        {
            if (!list.Contains(vector))
            {
                list.Add(vector);
                Count++;
            }
        }

        /// <summary>
        /// Ilość Rekordów w liście
        /// Record count in list
        /// </summary>
        public int Count { get; private set; }
        
                
        #endregion Metody obsługi danych wejściowych
                
        #region Metody i właściwości pomocnicze Algorytmu

        /// <summary>
        /// Returns formating string representing all CipherSample
        /// </summary>
        /// <returns></returns>
        string GetCipherSamplesStr()
        {
            string s = "PS: ";
            foreach(var r in list)
            {
                s += r.GetCipherSampleString() + "\t";
            }
            return s;
        }

        bool bBadPlace = false;
        /// <summary>
        /// Algorytm odświeża odpowiednio sloty w CommonSlots, jeśli record.IsOnRightPosition = false,
        /// Usuwa każdą cyfrę z listy Potencjalnych Cyfr ze slotów w których się te cyfry znajdują.  
        /// Algorytm jednorazowego użytku
        /// Algorithm tests record.IsOnRightPosition, and if it is set to false, collects digits from CipherSample and
        /// removes the digits from appropriate slots of CommonSlots. One use algorithm.
        /// </summary>
        /// <param name="rek">Record</param>
        void BadPlaceTestAlgo()
        {
            for(int i = 0; i < list.Count; i++) // --------------------- Dla wszystkich Rekordów ----------------
            {
                Record r = list[i];
                if (r.IsOnRightPosition) continue;                
                List<int?> cSample = r.CipherSample;
                for(int j = 0; j < cSample.Count ; j++) // ------------ Dla wszystkich miejsc w Próbce Szyfru ------------
                {
                    int? wrongNr = cSample[j];
                    if (wrongNr == null) continue;
                    List<int> l = new List<int>();
                    l.Add((int)wrongNr);
                    cs.UpdateOneSlot(l, j + 1);                                         
                }
            }
        }
                
        /// <summary>
        /// Sprawdza czy znaleziono już wszystkie cyfry (niekoniecznie znając ich prawidłowe  miejsce)
        /// Podejmuje decyzje o umieszczeniu ich w prawidłowych slotach
        /// Algorytm wielokrotnego użytku
        /// Test if correct digits list is full, then try put all digits in the list to properly slot
        /// Reuseble algorithm.
        /// </summary>
        void AllFoundTest()
        {
            if (correct.Count != cs.SlotCount) 
                return;
            List<int> a1 = cs.GetListForSlotNr(1);
            List<int> a2 = cs.GetListForSlotNr(2);
            List<int> a3 = cs.GetListForSlotNr(3);
            List<int> tmp = new List<int>();
            for(int i = 0 ; i < a1.Count; i++)
            {
                if (!correct.Contains(a1[i])) tmp.Add(a1[i]);
            }            
            cs.UpdateOneSlot(tmp, 1);

            tmp.Clear();

            for (int i = 0; i < a2.Count; i++)
            {
                if (!correct.Contains(a2[i])) tmp.Add(a2[i]);
            }
            cs.UpdateOneSlot(tmp, 2);

            tmp.Clear();
            
            for (int i = 0; i < a3.Count; i++)
            {
                if (!correct.Contains(a3[i])) tmp.Add(a3[i]);
            }
            cs.UpdateOneSlot(tmp, 3);            
        }

        /// <summary>
        /// Metoda sprawdza "logiczność" opisów Próbek Szyfru. W przypadku gdy natrafi na tę samą cyfrę w obu Próbkach Szyfru
        /// na tych samych pozycjach, bada spójność logiczną opisu obu Próbek Szyfru. Jeśli opisy są niespójne logicznie
        /// zwraca tę cyfrę. Kolejnym krokiem powinno być dodanie tej cyfry do zbioru cyfr niepoprawnych.
        /// Method test 2 Records for the same digit in the same slot, if data describing the digits are incoherent,
        /// returns the digit or null if not founded.
        /// </summary>
        /// <param name="first">Rekord</param>
        /// <param name="second">drugi (inny) rekord, other record</param>
        /// <returns>Liczbę z niepoprawną cyfrą, lub null, jeśli takiej cyfry metoda nie znalazła</returns>
        public static int? GetDigitWithMishmashDescriptionInSlot(Record first, Record second)
        {
            int? dig = null;
            for (int i = 0; i < 3; i++) //Wszystkie 3 sloty (pozycje cyfry)
            {
                dig = IsThereTheSameDigitInSlot(first, second, i);
                if (dig == null) continue;
                bool b = IsCorrectDigitsCountEqual(first, second);
                if (!b) continue;
                bool bo = IsThereTheSameDescriptionOfSlotCorrectness(first, second);
                if (bo) continue;
                return dig;
            }
            return null;
        }

        List<int> tested = new List<int>();
        bool bMishmash = false;
        /// <summary>
        /// Testuje Logiczną niespójność i jeśli występuje, wyrzuca cyfrę której ta niespójność dotyczy
        /// z wszystkich list CommonSlots
        /// Algorytm jednokrotnego stosowania
        /// See GetDigitWithMishmashDescriptionInSlot. If found such digit, removes the digit from all helper data.
        /// One use algorithm
        /// </summary>
        void LogicalMishmashTest()
        {
            int sorted = -1; //posortowana liczba wskazująca 2 indeksy Rekordów, gdzie cyfry nie mogą się powtarzać
            
            for (int i = 0; i < Count; i++) 
            {
                for (int j = 0; j < Count; j++)
                {
                    if (i == j) continue; // ------- unikamy powtarzania. Only unique record sets----------------

                    sorted = SortAndGet(i + 1, j + 1);//dodajemy 1 z powodu 0 na pierwszej pozycji
                    if (tested.Count > 0)
                    {
                        if (tested.Contains(sorted))  continue;                        
                        else tested.Add(sorted);                                                    
                    }
                    else tested.Add(sorted);                        
                    //--------------------------- koniec unikamy powtarzania ---------------
                    
                    int? dig = GetDigitWithMishmashDescriptionInSlot(list[i], list[j]); // 6
                    if (dig != null)
                    {                                                        
                        // ------------- usuwamy z tmp Slots Rekordów niepoprawną cyfrę --------
                        List<int> ll = new List<int>();
                        ll.Add((int)dig);                            
                        cs.UpdateAllSlots(ll);
                        RemoveWrongNrsFromAllCipherSample(ll);                        
                    }
                }//for(j
            }//for (i 
        }

        /// <summary>
        /// Sprawdza czy w tymczasowych slotach o numerze slotNr dla obu rekordów jest ta sama cyfra.
        /// Test for the same digit in the same slots both compared Records
        /// Returns null if there are different digits, or number
        /// </summary>
        /// <param name="first">Rekord</param>
        /// <param name="second">Inny Rekord. Other Record</param>
        /// <param name="slotNr">Numer slotu do sprawdzenia. Slot nr to test</param>
        /// <returns>Null jeśli są różne cyfry, w przeciwnym wypadku zwraca wartość tej cyfry jako int</returns>
        private static int? IsThereTheSameDigitInSlot(Record first, Record second, int slotNr)
        {            
            List<int?> csF = first.CipherSample;
            List<int?> csS = second.CipherSample;
            if (csF[slotNr] == csS[slotNr]) return csS[slotNr];
            return null;            
        }

        /// <summary>
        /// Zwraca true, jeśli w obu rekordach ilość poprawnych cyfr jest taka sama
        /// Returns true, if in both records there is the same value property MatchedCount
        /// </summary>
        /// <param name="first">Rekord</param>
        /// <param name="second">Inny Rekord. Other Record</param>
        /// <returns>bool</returns>
        private static bool IsCorrectDigitsCountEqual(Record first, Record second)
        {
            return first.MatchedCount == second.MatchedCount;
        }

        /// <summary>
        /// Zwraca true, jeśli w obu rekordach flaga wskazująca na poprawność pozycji dla poprawnych cyfr jest taka sama
        /// Returns true, if in both Records, IsOnRightPosition property value are the same.
        /// </summary>
        /// <param name="first">Rekord</param>
        /// <param name="second">Inny Rekord. Other Record</param>
        /// <returns>bool</returns>
        private static bool IsThereTheSameDescriptionOfSlotCorrectness(Record first, Record second)
        {
            return first.IsOnRightPosition == second.IsOnRightPosition;
        }

        /// <summary>
        /// Zwraca uporządkowany zestaw stworzony z 2 liczb
        /// Z cyfr 3 i 1 zwraca 13
        /// z cyfr2 i 4 zwraca 24
        /// Returns sorted set of integers representing digits
        /// </summary>
        /// <param name="first">liczba. Number</param>
        /// <param name="second">inna liczba. Other number</param>
        /// <returns></returns>
        private static int SortAndGet(int first, int second)
        {            
            int a = -1;
            int b = -1;
            if (first < second)
            {
                a = first;
                b = second;
            }
            else
            {
                a = second;
                b = first;
            }            
            return (a*10) + b;
        }

        /// <summary>
        /// Odświeża sloty w przypadku, gdy algorytm wykryje niepoprawny numer
        /// Updates CipherSample if algo found incorrect number. Sets the digit to null.
        /// </summary>
        /// <param name="wrongNr">Niepoprawny nr. Incorrect nr</param>
        /// <param name="rekord">Record</param>                
        void UpdateAfterHitrWrongNr(int? wrongNr, ref Record rekord)
        {
            List<int?> slo = rekord.CipherSample;
            for (int l = 0; l < 3; l++) // dla pozycji w Slotach
            {
                if (slo[l] == wrongNr)
                {
                    slo[l] = null;
                    rekord.CipherSample = slo;                    
                }               
            }            
        }

        /// <summary>
        /// Dodaje do listy poprawnych cyfr cyfrę goodNr
        /// Adds goodNr to list of correct numbers.
        /// </summary>
        /// <param name="goodNr">Poprawna cyfra w szyfrze. Correct Nr</param>
        void UpdateAfterHitGoodNr( int? goodNr )
        {
            if(!correct.Contains((int)goodNr)) correct.Add((int)goodNr);            
        }
        
        /// <summary>
        /// Zwraca string reprezentujący listy w poszczególnych slotach CommonSlots
        /// Returns formatted string representing list in slots of CommonSlots
        /// </summary>
        /// <returns>string</returns>
        string GetSlotsStrings()
        {
            string s = "";
            for(int i = 1; i < 4; i++)
            {
                if (i > 1) s += "\t  ";
                List<int> l = cs.GetListForSlotNr(i);
                for(int j = 0; j < l.Count; j++ )
                {                    
                    s += l[j];                    
                    if (j == l.Count - 1) s += "\r\n";
                }
            }
            return s;
        }
        
        /// <summary>
        /// Test sprawdza czy wartość rekord.MatchedCount == slots.Count, oznacza to że,
        /// cyfry w slotach są poprawne (niekoniecznie na dobrych miejscach). Zapisuje te cyfry w słownikach
        /// wrongPosition i goodPosition w zależności od flagi Record.IsOnCorrectPosition
        /// jeśli słownik jest niepusty zapisuje te cyfry do listy poprawnych cyfr (correct) oraz
        /// jeśli cyfra jest na złej pozycji, to usuwa ją z listy znajdującej się w odpowiednim slocie
        /// jeśli cyfra jest na dobrej pozycji dopisuje ją do listy correct i usuwa ją z list znajdujących się w innych slotach
        /// Algorytm do wielokrotnego powtórzenia w kolejnych pzrejściach pętli głównego Algorytmu
        /// Tests values properties rekord.NotNullCount and rekord.MatchedCount anf if are equal (and > 0)...
        /// Stores the digits in dictionaries wrongPositions or goodPositions depends from IsOnRightPosition property value.
        /// If dictionary is no empty, stores the digits to list of correct numbers, and...
        /// If the digit is on incorrect position, then removes it from corresponding slot of CommonSlots,
        /// If the digit is on correct position then removes it from inappropriate slots of CommonSlots.
        /// Reusable algorithm
        /// </summary>        
        void EqualTest( )
        {
            for (int z = 0; z < list.Count; z++) //wszystkie Rekordy
            {
                Record rekord = list[z];
                List<int?> slo = rekord.CipherSample;
                Dictionary<int, int> wrongPosition = new Dictionary<int, int>(); // tymczasowy słownik złych pozycji
                Dictionary<int, int> goodPosition = new Dictionary<int, int>();  // tymczasowy słownik dobrych pozycji
                int nnc = rekord.NotNullCount();
                if (nnc == 0) return;
                if (rekord.MatchedCount == nnc)// w slocie jest tyle samo cyfr, co IlePoprawnych
                {
                    for (int i = 0; i < 3; i++)//Szukamy tych cyfr i zapisujemy w stosownych słownikach tymczasowych
                    {
                        int? inte = slo[i];
                        if (inte != null)
                        {
                            UpdateAfterHitGoodNr(inte);
                            if (rekord.IsOnRightPosition)
                                goodPosition.Add(i + 1, (int)inte);
                            else
                                wrongPosition.Add(i + 1, (int)inte);
                        }
                    }
                }

                if (wrongPosition.Count > 0) // sprawdzamy czy w tymczasowym słowniku są jakieś wpisy
                {
                    foreach (var v in wrongPosition)
                    {
                        List<int> l = new List<int>();
                        l.Add(v.Value);
                        cs.UpdateOneSlot(l, v.Key);
                        
                    }
                }

                if (goodPosition.Count > 0) // sprawdzamy czy w tymczasowym słowniku są jakieś wpisy
                {
                    foreach (var v in goodPosition)
                    {
                        cs.UpdateFoundCorrectNr(v.Value, v.Key);                        
                    }
                }
            }
        }

        /// <summary>
        /// Usuwa wszystkie niepoprawne cyfry z Próbek Szyfru każdego rekordu
        /// Sets to null incorrect digit in CipherSample all Records.
        /// </summary>
        /// <param name="wrongNrs">Lista niepoprawnych cyfr. Incorrect nrs list</param>
        void RemoveWrongNrsFromAllCipherSample(List<int> wrongNrs)
        {
            foreach(var r in list)
            {
                foreach (var i in wrongNrs)
                    r.UpdateBadNr(i);
            }
        }

        bool bRecord4 = false;
        /// <summary>
        /// WrongAllCipherSampleTest
        /// Sprawdza czy Record.MatchedCount == 0, jeśli tak
        /// Tworzy listę nieprawidłowych cyfr i usuwa te cyfry z wszystkich list CommonSlots
        /// Usuwa też te cyfry z CipherSample w każdym Rekordzie
        /// Algorytm do jednokrotnego zastosowania
        /// Tests if Record's property MatchedCound is equal zero, and then...
        /// removes all digits record's CipherSample from all data.
        /// Reusable Algorithm
        /// </summary>
        void Record4Algo()
        {
            List<int> wrongNrs = new List<int>();
            for (int i = 0; i < list.Count; i++)// All Records
            {
                if (list[i].MatchedCount == 0)// Zadna cyfra nie jest poprawna
                {
                    List<int?> slo = list[i].CipherSample;
                    for (int j = 0; j < 3; j++)
                    {
                        int? lic = slo[j];
                        if (lic != null)
                        {                            
                            slo[j] = null;
                            if (!wrongNrs.Contains((int)lic)) wrongNrs.Add((int)lic);
                        }
                    }
                    list[i].CipherSample = slo;
                }
            }

            if (wrongNrs.Count > 0) // usuwamy z list potencjalnych cyfr złe cyfry dla wszystkich 3 slotów
            {
                cs.UpdateAllSlots(wrongNrs.ToList());
                RemoveWrongNrsFromAllCipherSample(wrongNrs);
            }
        }

        /// <summary>
        /// Algorytm sprawdza dla kazdej cyfry z listy correct czy ta cyfra znajduje się tylko w jednym slocie CommonSlots
        /// Jeśli tak, to usuwa tę cyfrę z innych slotów.
        /// Algorytm wielokrotnego użytku
        /// Test every digit of correct list if it has only one instance in CommonSlots, if yes...
        /// removes the digit from other slots of CommonSlots.
        /// Reusable Algorithm
        /// </summary>
        void FoundOnlyOneSlotWithCorrectNrAlgo()
        {            
            foreach(var c in correct)
            {
                int nr = GetUniqueSlotWithCorrectNr(c);
                if (nr == 0) continue;
                cs.UpdateFoundCorrectNr(c, nr);                
            }
        }

        /// <summary>
        /// Zwraca nr slotu z przedziału [1..3] jeśli tylko w jego liście jest correctNr
        /// Returns slot nr, that is unique slot containing correctNr or 0 if there no so unique slot
        /// </summary>
        /// <param name="correctNr">poprawna cyfra. Correct nr</param>
        /// <returns>0 jeśli nie ma takiego slotu, lub nr slotu</returns>
        int GetUniqueSlotWithCorrectNr(int correctNr)
        {
            int licz = 0;
            List<int> l;
            Dictionary<int, int> dic = new Dictionary<int, int>();
            for(int i = 1; i <= cs.SlotCount; i++)
            {
                l = cs.GetListForSlotNr(i);                
                if (l.Contains(correctNr))
                {
                    if (licz == 1) return 0;
                    licz++;
                    dic.Add(i, i);
                }
            }
            int ret = 0;
            foreach(var r in dic)
            {
                ret = r.Value;
            }
            return ret;
            
        }

        /// <summary>
        /// Algorytm sprawdza czy został już tylko jeden nieobsadzony slot i wyszukuje taki CipherSample, 
        /// w którym nie ma już odnalezionych cyfr i testuje czy cyfry w tym slocie znajdują się na liście
        /// nieobsadzonego slotu w CommonSlots. Jeśli pasuje tylko jedna cyfra, to wstawia ją w nieobsadzony slot i kasuje inne
        /// To jest jednorazowy algorytm, kończący zadanie ( o ile się powiedzie).
        /// Tests if there is unique slot in CommonSlots, that has more than 1 digits in list, then...
        /// search for CipherSample containing any digits different than digits in correct list,
        /// If in the slot is only one instance SUCH digit, removes other digits from the slot.
        /// One use algorithm, ending program (?)
        /// </summary>
        void LastNrTest()
        {
            if (cs.FoundedCount() != 2) return;
            List<int> l = cs.GetListForSlotNr(cs.NotCorrectSlotNr);
            List<int> buf = new List<int>();
            foreach(var r in list)
            {
                if (r.MatchedCount == 0) continue;
                for(int i = 1; i < r.CipherSample.Count; i++)
                {
                    int? ci = r.CipherSample[i];
                    if (r.CipherSample[i] == null) ci = -1;
                    if( l.Contains((int)ci) )
                    {
                        buf.Add((int)r.CipherSample[i]);
                    }
                }
            }
            if(buf.Count==1)
            {
                cs.UpdateFoundCorrectNr(buf[0], cs.NotCorrectSlotNr);
            }
        }

        #endregion Metody pomocnicze Algorytmu

        #region MainAlgorytm

        bool bbeforeAlgo = false;

        /// <summary>
        /// Główny algorytm. Algorytm niczego nie waliduje, nie sprawdza czy dane wejściowe istnieją
        /// W przypadku wyjątku kończy działanie. Opis błędu wysyła do ERR
        /// Main loop of algorithm. No validation, noe exception testing, no testing existing input data...        
        /// </summary>
        private void MainAlgo()
        {            
            try
            {
               tested.Clear();               
               int passes = 0;
               for(;;)
               {
                   if (cs.IsSolved)
                   {
                       AlgoLog += "\r\nMamy rozwiązanie! " + cs.SolvedString() + " po " + passes + " przejściach petli.";
                       return;
                   }
                   
                   if (passes == 20) return; // zabezpieczenie
                   passes++;
                   if (!bbeforeAlgo)
                   {
                       UpdateAlgoLog(passes, "Przed Algo");
                       bbeforeAlgo = true;
                   }
                   if (!bMishmash) //tylko raz używamy ten algo
                   {
                       LogicalMishmashTest(); // Rekord 1 i 2            --------------------- mishmash ----
                       bMishmash = true;
                       UpdateAlgoLog(passes, "LogicalMishmashTest()");
                   }
                                         
                   EqualTest();                   //                    ---------------------- equal --------
                   UpdateAlgoLog(passes, "EqualTest()");
                   
                   
                   if (!bRecord4)
                   {
                       Record4Algo();                       //          --------------------- Record4 --------
                       UpdateAlgoLog(passes, "Record4Algo");
                       bRecord4 = true;
                   }               
                                  
                   if (!bBadPlace)
                   {
                       BadPlaceTestAlgo();                              //------------------ BadPlace --------
                       UpdateAlgoLog(passes, "BadPlaceTestAlgo");
                       bBadPlace = true;
                   }
                                  
                   AllFoundTest();                                      // ----------------- AllFound ---------
                   UpdateAlgoLog(passes, "AllFoundTest");
                   
                   FoundOnlyOneSlotWithCorrectNrAlgo();                 // ----------------- FoundOnlyOne -----
                   UpdateAlgoLog(passes, "FoundOnlyOneSlotWithCorrectNrAlgo()");

                   LastNrTest();                                        // ----------------- LastNr -----------
                   UpdateAlgoLog(passes, "LastNrTest");
               }//for(;;)
            }
            catch(Exception ex)
            {
                ERR = ex.ToString();
            }
        }

        /// <summary>
        /// Updates algorithm log
        /// </summary>
        /// <param name="pass">pass nr of loop</param>
        /// <param name="algoName">Updates after algo name</param>
        void UpdateAlgoLog(int pass, string algoName)
        {
            string lf = "\r\n";
            string t = "\t";
            string mess = GetCorrectState();
            if (!string.IsNullOrEmpty(mess)) mess += lf;
            AlgoLog += "Pass=" + pass + t + algoName + t + cs.GetState() + lf + mess  + GetCipherSamplesStr() + lf;
        }

        /// <summary>
        /// Returns string representing correct list elements
        /// </summary>
        /// <returns>string</returns>
        string GetCorrectState()
        {
            string s = "Poprawne= ";
            if (correct.Count == 0) return "";
            foreach (var v in correct)
                s += v;
            return s;
        }

        /// <summary>
        /// Gets last error
        /// </summary>
        /// <returns></returns>
        public string GetLastError()
        {
            return ERR;
        }

        /// <summary>
        /// Pomocniczy string opisujący kroki algorytmu
        /// Gets string representing algorithm log
        /// </summary>
        /// <returns>string</returns>
        public string GetAlgoLog()
        {
            return AlgoLog;
        }

        #endregion MainAlgorytm
    }
}
