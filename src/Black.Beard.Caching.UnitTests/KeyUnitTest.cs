using Black.Beard.Caching.Runtime;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Black.Beard.Caching.UnitTests
{

    /// <summary>
    /// sachant qu on tire toutes les z milisecondes un nombre au hasard x fois.
    /// Quel est le % de chance pour que le reste de la division par la taille du tableau _locks donne un résultat identique au court des dernières y miliseconds écoulées
    /// 
    /// x                   -> nombre de fois                       (Plus le chiffre est grand plus y a de chance que cela se produise)
    /// y                   -> temps de contention                  (sur un temps de traitement fix, plus le temps de contention est grand et plus le nombre de lock posés va etre grands)
    /// z                   -> temps écoulé entre chaque tirage     (plus le temps entre les tirage va être petit et plus y a de chance que des contentions se produisent)
    /// concurrencyLevel    -> taille du tableau de lock            (plus le tableau est petit et plus les locks vont être distribués sur le surface plus petite et donc plus la chance de lockage va être grande)
    /// 
    /// Quel est le meilleur rapport concurrencyLevel / y / z
    /// Plus le nombre de fois x ou je tire est élevé et plus fréquence entre chaque tirage est courte et plus j aurais besoin d'avoir une paralelisation importante.
    /// 
    /// Tests :
    ///     Pour un concurrencyLevel fixe, faire varier y en incrément
    ///     Pour un concurrencyLevel fixe, faire varier z en décrément
    /// 
    /// </summary>
    [TestClass]
    public class KeyUnitTest
    {

        /// <summary>
        /// Simulates the concurrency keys unit test.
        /// </summary>
        [TestMethod]
        public void Simulate_Concurrency_Keys_UnitTest()
        {

            int concurrencyLevel = 1000;
            int x_runTimes = 20000;
            int z_frequence = 1;

            // int y_durationProcess = 50;      // 14199, 18902
            int y_durationProcess = 100;        // 

            var i = Run(concurrencyLevel, x_runTimes, y_durationProcess, z_frequence);

        }

        private static int Run(int concurrencyLevel, int x, int y, int z)
        {

            object _lock = new object();

            int count_locks = 0;
            var _locks = new lockItem[concurrencyLevel];
            for (int i = 0; i < concurrencyLevel; i++)
                _locks[i] = new lockItem();

            Action<Guid> fnc = (g) =>
            {

                int lockNo = (g.GetHashCode() & 0x7fffffff) % concurrencyLevel;
                var item = _locks[lockNo];

                if (item.Count > 1)
                    count_locks++;

                lock (_lock)
                    item.Count++;

                Thread.Sleep(y);

                lock (_lock)
                    item.Count--;

            };


            for (int i = 0; i < x; i++)
            {
                Task.Factory.StartNew(() => fnc(Guid.NewGuid()));
                Thread.Sleep(z);
                if (count_locks > 0)
                {
                    return i;
                }
            }

            while (count_locks > 0)
                Task.Yield();

            return 0;

        }

        /// <summary>
        /// Evaluate howmany key are concurrents
        /// </summary>
        [TestMethod]
        public void Concurrency_Keys_UnitTest1()
        {

            int concurrencyLevel = 1000;

            var _locks = new lockItem[concurrencyLevel];
            for (int i = 0; i < concurrencyLevel; i++)
                _locks[i] = new lockItem();

            for (int i = 0; i < concurrencyLevel; i++)
            {

                var key = Guid.NewGuid();

                int hashcode = key.GetHashCode();                               // and logic 0x7fffffff -> remove sign and result is positive
                int lockNo = (hashcode & 0x7fffffff) % concurrencyLevel;        // modulo -> find an integer less than array length 
                Assert.IsTrue(lockNo >= 0 && lockNo < concurrencyLevel);        // 

                var item = _locks[lockNo];

                item.Count++;

            }

            int count_unuseds = 0;
            int count_locks = 0;
            int max_locks = 0;

            for (int lockNo = 0; lockNo < concurrencyLevel; lockNo++)
            {
                var item = _locks[lockNo];
                max_locks = Math.Max(max_locks, item.Count);
                if (item.Count == 0)
                    count_unuseds++;
                else if (item.Count > 1)
                {
                    count_locks++;
                }
            }


        }


        private class lockItem
        {
            public volatile object _syncLock = new Object();
            public int Count = 0;
        }


    }
}
