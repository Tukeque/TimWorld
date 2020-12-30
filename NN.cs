using System;

namespace TimWorld
{
    class Neuron
    {
        byte[] weights;
        public byte value;
        byte futurevalue;

        public Neuron(byte[] ws, byte v)
        {
            weights = ws;
            value = v;
        }

        public Neuron(int weightCount, byte v)
        {
            weights = new byte[weightCount];

            for (int i = 0; i < weightCount; i++)
            {
                weights[i] = SpecialMath.FloatToByte(Extra.NextFloat());
            }

            value = v;
        }

        public void Update()
        {
            value = futurevalue;
            futurevalue = 0;
        }

        public void Iterate(byte[] neurons) // bad iterate lol
        {
            float total = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                float a = SpecialMath.ByteToFloat(neurons[i]);
                a *= weights[i];
                total += a;
            }

            futurevalue = SpecialMath.FloatToByte(SpecialMath.Sigmoid(total));
        }

        public void Iterate(Neuron[] neurons, Neuron neuron)
        {
            float total = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                if (neuron != neurons[i])
                {
                    float a = SpecialMath.ByteToFloat(neurons[i].value);
                    a *= weights[i];
                    total += a;
                }
            }

            futurevalue = SpecialMath.FloatToByte(SpecialMath.Sigmoid(total));
        }
    }

    class NN
    {
        public Neuron[] neurons;
        public int neuronCount;

        public NN(int thinkingneurons, int workingneurons)
        {
            Random random = new Random();
            neuronCount = thinkingneurons + workingneurons;
            neurons = new Neuron[neuronCount];

            for (int i = 0; i < neuronCount; i++)
            {
                neurons[i] = new Neuron(neuronCount - 1, SpecialMath.FloatToByte((float)(random.NextDouble() * 2) - 1));
            }
        }

        public void Iterate()
        {
            for (int i = 0; i < neuronCount; i++)
            {
                neurons[i].Iterate(neurons, neurons[i]);
            }

            for (int i = 0; i < neuronCount; i++)
            {
                neurons[i].Update();
            }
        }
    }
}
