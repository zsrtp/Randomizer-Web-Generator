namespace TPRandomizer.Util
{
    using System;
    using System.Collections.Generic;

    public class VoseAlgorithm
    {
        public static VoseInstance<T> createInstance<T>(List<KeyValuePair<double, T>> weightsIn)
        {
            if (weightsIn == null)
                throw new Exception("`weightsIn` must be non-null.");

            List<KeyValuePair<double, T>> weightedList = new();
            foreach (KeyValuePair<double, T> pair in weightsIn)
            {
                if (pair.Key > 0)
                    weightedList.Add(pair);
            }

            if (weightedList.Count == 0)
            {
                return new VoseInstance<T>(Array.Empty<double>(), Array.Empty<int>(), 0, new());
            }

            List<double> weights = new();
            foreach (KeyValuePair<double, T> pair in weightedList)
            {
                weights.Add(pair.Key);
            }

            int probLength = weights.Count;

            double total = 0;
            for (int i = 0; i < probLength; i++)
            {
                total += weights[i];
            }

            int[] alias = new int[probLength];
            double[] probability = new double[probLength];

            double avg = total / probLength;

            List<int> small = new();
            List<int> large = new();

            for (int i = 0; i < probLength; i++)
            {
                if (weights[i] >= avg)
                    large.Add(i);
                else
                    small.Add(i);
            }

            while (small.Count > 0 && large.Count > 0)
            {
                int less = small[small.Count - 1];
                small.RemoveAt(small.Count - 1);
                int more = large[large.Count - 1];
                large.RemoveAt(large.Count - 1);

                probability[less] = weights[less] * probLength;
                alias[less] = more;

                weights[more] = weights[more] + weights[less] - avg;

                if (weights[more] >= avg)
                    large.Add(more);
                else
                    small.Add(more);
            }

            while (small.Count > 0)
            {
                int val = small[small.Count - 1];
                small.RemoveAt(small.Count - 1);
                probability[val] = total;
            }

            while (large.Count > 0)
            {
                int val = large[large.Count - 1];
                large.RemoveAt(large.Count - 1);
                probability[val] = total;
            }

            return new VoseInstance<T>(probability, alias, total, new(weightedList));
        }
    }

    public class VoseInstance<T>
    {
        private List<double> probability;
        private List<int> alias;
        private double total;
        private List<KeyValuePair<double, T>> weightedList;

        public VoseInstance(
            double[] probability,
            int[] alias,
            double total,
            List<KeyValuePair<double, T>> weightedList
        )
        {
            this.probability = new(probability);
            this.alias = new(alias);
            this.total = total;
            this.weightedList = weightedList;
        }

        private void replaceFromInstance(VoseInstance<T> newInstance)
        {
            this.probability = newInstance.probability;
            this.alias = newInstance.alias;
            this.total = newInstance.total;
            this.weightedList = newInstance.weightedList;
        }

        private int getIndex(Random rnd)
        {
            int column = rnd.Next(probability.Count);
            bool coinToss = (rnd.NextDouble() * total) < probability[column];
            return coinToss ? column : alias[column];
        }

        public T NextAndKeep(Random rnd)
        {
            int index = getIndex(rnd);
            return weightedList[index].Value;
        }

        public T NextAndRemove(Random rnd)
        {
            int index = getIndex(rnd);
            T item = weightedList[index].Value;

            weightedList.RemoveAt(index);
            // Replace using new instance.
            VoseInstance<T> newInstance = VoseAlgorithm.createInstance<T>(weightedList);
            replaceFromInstance(newInstance);

            return item;
        }

        public bool HasMore()
        {
            return weightedList.Count > 0;
        }
    }
}
