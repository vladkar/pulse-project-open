using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.ControlsClient;
using City.Snapshot;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;

namespace City.Panel
{
    public class PanelManager
    {
        // key: panel id (1,2,3,4)
        public IDictionary<int, IPulsePanel> Panels { private set; get; }

        // key: control id
        //        public IDictionary<int, IPulseClient> Clients { private set; get; }
        public IDictionary<int, IPulseControl> Controls { private set; get; }
        public IDictionary<int, List<Frame>> ControlElements { private set; get; }

        public ServerInfo Config { private set; get; }
        private Game _engine;

        public PanelManager(Game gameEngine)
        {
            _engine = gameEngine;
            Panels = new Dictionary<int, IPulsePanel>();
//            Clients = new Dictionary<int, IPulseClient>();
            Controls = new Dictionary<int, IPulseControl>();
            ControlElements = new Dictionary<int, List<Frame>>();
        }

        //initialize with empty controls
        public IDictionary<int, IPulseControl> LoadOrUpdateControls(ServerInfo si)
        {
            Config = si;
            var newClients = new Dictionary<int, IPulseControl>();
            ControlElements.Clear();
            // clean previous clients
            foreach (var panel in Panels)
            {
                ControlElements.Add(panel.Key, new List<Frame>());
            }


            //add new clients
            foreach (var controlInfo in si.Controls)
            {
                var control = !Controls.ContainsKey(controlInfo.Key) ? ControlFactory.GetClientControl(controlInfo.Value.Name, controlInfo.Value.Scenario) : Controls[controlInfo.Key];

                newClients[controlInfo.Key] = control;
                
                control.Initialize(_engine);

                foreach (var view in control.Views)
                {
                    var controlElements = view.Value.AddControlsToUI();
                    var viewInfo = controlInfo.Value.Views[view.Key];
                    var panel = Panels.First(p => p.Key == viewInfo.Panel);

                    ControlElements[panel.Key].Add(controlElements);

                    panel.Value.Views.Add(view.Value);
                    // dont forget to unregister if necessary
                    view.Value.ViewLayer	= panel.Value.PanelLayer;
	                view.Value.Panel		= panel.Value;
                }

                control.Load(controlInfo.Value);
            }

            Controls = newClients;

            return Controls;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var panel in Panels.Values)
            {
                panel.Update(gameTime);
            }
        }
    }
}
