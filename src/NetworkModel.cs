using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ConsoleTableExt;

namespace SimpleNeuralNet {

	public class NetworkModel {

		public List<NeuralLayer> Layers {
			get; set;
		}

		public NetworkModel() {
			Layers = new List<NeuralLayer>();
		}

		public void AddLayer(NeuralLayer layer) {
			int dendriteCount = 1;

			if (Layers.Count > 1) {
				dendriteCount = (from l in Layers
								 select l.Neurons).Count();
			}

			foreach (var element in layer.Neurons) {
				for (int i = 0; i < dendriteCount; i++) {
					element.Dendrites.Add(new Dendrite());
				}
			}
		}

		public void Build() {
			int i = 0;

			foreach (var layer in Layers) {
				if (i >= (Layers.Count - 1)) {
					break;
				}

				var nextLayer = Layers[i + 1];
				CreateNetwork(layer, nextLayer);

				i++;
			}
		}

		public void Print() {
			DataTable dt = new DataTable();

			dt.Columns.Add("Name");
			dt.Columns.Add("Neurons");
			dt.Columns.Add("Weight");

			foreach (var element in Layers) {
				DataRow row = dt.NewRow();
				row[0] = element.Name;
				row[1] = element.Neurons.Count;
				row[2] = element.Weight;

				dt.Rows.Add(row);
			}

			//Use Nu-Get to install ConsoleTableExt package.
			ConsoleTableBuilder builder = ConsoleTableBuilder.From(dt);
			builder.ExportAndWrite();
		}

		public void Train(NeuralData X, NeuralData Y, int iterations, double learningRate = 0.1D) {
			int epoch = 1;

			//Loop through each iteration
			while (iterations > epoch) {
				//Get the input layers
				var inputLayer = Layers[0];
				List<double> outputs = new List<double>();

				//Loop through the record
				for (int i = 0; i < X.Data.Length; i++) {
					//Set the input data into the first layer
					for (int j = 0; j < X.Data[i].Length; j++) {
						inputLayer.Neurons[j].OutputPulse.Value = X.Data[i][j];
					}

					//Fire all the neurons and collect the output
					ComputeOutput();
					outputs.Add(Layers.Last().Neurons.First().OutputPulse.Value);
				}

				//Check the accuracy score against Y with the actual output
				double accuracySum = 0;
				int y_counter = 0;

				outputs.ForEach((x) => {
					if (x == Y.Data[y_counter].First()) {
						accuracySum++;
					}

					y_counter++;
				});

				//Optimize the synaptic weights
				OptimizeWeights(accuracySum / y_counter);
				Console.WriteLine("Epoch: {0}, Accuracy: {1} %", epoch, (accuracySum / y_counter) * 100);
				epoch++;
			}
		}

		private void CreateNetwork(NeuralLayer connectingFrom, NeuralLayer connectingTo) {
			foreach (var to in connectingTo.Neurons) {
				foreach (var from in connectingFrom.Neurons) {
					to.Dendrites.Add(new Dendrite() { InputPulse = to.OutputPulse, SynapticWeight = connectingTo.Weight });
				}
			}
		}

		private void ComputeOutput() {
			bool first = true;

			foreach (var layer in Layers) {
				//Skip first layer because it's input
				if (first) {
					first = false;
					continue;
				}

				layer.Forward();
			}
		}

		private void OptimizeWeights(double accuracy) {
			float lr = 0.1f;

			//Skip if accuracy reached 100%
			if (accuracy == 1) {
				return;
			}

			if (accuracy > 1) {
				lr = -lr;
			}

			//Update weights for all layers
			foreach (var layer in Layers) {
				layer.Optimize(lr, 1);
			}
		}
	}
}
