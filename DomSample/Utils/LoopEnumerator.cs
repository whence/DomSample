using System;
using System.Collections;
using System.Collections.Generic;

namespace DomSample.Utils
{
    public class LoopEnumerator<T> : IEnumerator<T>
    {
        #region fields
        private readonly IList<T> list;
        private readonly int startIndex;
        private readonly int loopCount;
        private int index;
        private int currentLoopCount;
        #endregion

        #region properties
        public T Current { get; private set; }
        #endregion

        #region constructors
        public LoopEnumerator(IList<T> list, int startIndex, int loopCount)
        {
            if (list == null) 
                throw new ArgumentNullException("list");
            this.list = list;

            if (startIndex < 0 || startIndex >= list.Count)
                throw new ArgumentOutOfRangeException("startIndex");
            this.startIndex = startIndex;

            if (loopCount <= 0)
                throw new ArgumentOutOfRangeException("loopCount", "must be positive");
            this.loopCount = loopCount;

            this.index = 0;
            this.Current = default(T);
        }
        #endregion

        #region methods
        public void Reset()
        {
            this.Current = default(T);
            this.index = startIndex;
            this.currentLoopCount = -1;
        }

        public bool MoveNext()
        {
            if (this.index == this.startIndex)
            {
                this.currentLoopCount++;
                if (this.currentLoopCount == this.loopCount)
                    return false;
            }
            
            this.Current = list[this.index];
            this.index = Maths.StepInLoop(this.index, 1, 0, list.Count - 1);
            
            return true;
        }
        #endregion

        #region interface members
        object IEnumerator.Current
        {
            get { return Current; }
        }

        void IDisposable.Dispose()
        {
        }
        #endregion
    }
}
