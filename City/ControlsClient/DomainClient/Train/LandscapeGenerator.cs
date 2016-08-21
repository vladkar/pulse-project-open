using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Common;
using Fusion.Core.Mathematics;

namespace City.ControlsClient.DomainClient.Train
{
	public class LandscapeGenerator
	{
		public List<ModelLayer> Models { set; get; }
		public int	ActiveObjectsCount;
	    public bool IsSpawnActive = true;

		public float Velocity = 25.0f;

		Random r = new Random();

		public float MinDist = -500000;
		public float MaxDist = 50000;

		private int dispersion = 1000;

		List<Queue<int>>	FreeModels;
		List<List<int>>		ActiveModels;

		public float[] TimeUntilNextSpawn;
		float maxRespawnTime = 25.0f;

		public LandscapeGenerator(List<ModelLayer> models)
		{
			Models = models;

			FreeModels		= new List<Queue<int>>();
			ActiveModels	= new List<List<int>>();


			TimeUntilNextSpawn = new float[models.Count];
			for(int i = 0; i < TimeUntilNextSpawn.Length; i++) {
				TimeUntilNextSpawn[i] = r.NextFloat(1.0f, 3.0f);
            }


			int j = 0;
			foreach(var m in models) {
				ActiveModels.Add(new List<int>());
				FreeModels.Add(new Queue<int>());

				for(int i = 0; i < m.InstancedDataCPU.Length; i++) {
					m.InstancedDataCPU[i].World = Matrix.Translation(new Vector3(MinDist, j * 100, 0));
					m.InstancedDataCPU[i].Color = new Color4(1.0f, 1.0f, 1.0f, 0.0f);

					FreeModels[j].Enqueue(i);
                }

				j++;
			}

		}


		int side = 1;
		public void Update(TimeSpan diff)
		{
			if(IsSpawnActive) { 
				for(int i = 0; i < TimeUntilNextSpawn.Length; i++) {
					TimeUntilNextSpawn[i] -= (float)diff.TotalSeconds;

					if(TimeUntilNextSpawn[i] <= 0.0f) {
						TimeUntilNextSpawn[i] = r.NextFloat(5.0f, maxRespawnTime);

						if(FreeModels[i].Any()) {
							var model = FreeModels[i].Dequeue();

							ActiveModels[i].Add(model);
							
							Models[i].InstancedDataCPU[model].World = Matrix.Scaling(r.NextFloat(1.0f, 2.0f)) * Matrix.Translation(new Vector3(MinDist, 1800*side + r.Next(-dispersion, dispersion), 0));
							Models[i].InstancedDataCPU[model].Color = Color4.White;

							side *= -1;
						}
				    }
				}
			}

			for(int i = 0; i < Models.Count; i++) {
				var toRemoveList = new List<int>();

				foreach (var model in ActiveModels[i]) {
					var pos = Models[i].InstancedDataCPU[model].World.TranslationVector;
					pos.X += Velocity * (float)diff.TotalSeconds;

					Models[i].InstancedDataCPU[model].World.TranslationVector = pos;

					if(pos.X > MaxDist) {
						toRemoveList.Add(model);
					}
				}

				foreach (var rem in toRemoveList) {
					ActiveModels[i].Remove(rem);
					FreeModels[i].Enqueue(rem);
				}
			}

		}
	}
}
