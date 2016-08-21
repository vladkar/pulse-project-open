using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using City.UIFrames.Converter.Attribute;

namespace City.UIFrames.Converter.Models
{
    [ClassAttribute("Pulse Agents")]
    public class PulseAgentUI : INotifyPropertyChanged
    {
        private float _slider;
        private string _fps;
        private string _repulsiveAgentField;
        private string _repulsiveAgentFactorField;
        private string _repulsiveObstacleField;
        private string _repulsiveObstacleFactorField;
        private bool _showAgentId;
        private bool _showSocialForce;


        [EditBoxAttribute("AC", "0.12")]
        public string RepulsiveAgentField
        {
            get { return _repulsiveAgentField; }
            set
            {
                if (!value.Equals(_repulsiveAgentField))
                {
                    _repulsiveAgentField = value;
                    OnPropertyChanged("sf");
                }    
            }
        }

        [EditBoxAttribute("AF", "70")]
        public string RepulsiveAgentFactorField
        {
            get { return _repulsiveAgentFactorField; }
            set {
                if (!value.Equals(_repulsiveAgentFactorField))
                {
                    _repulsiveAgentFactorField = value;
                    OnPropertyChanged("sf");
                }
            }
        }

        [EditBoxAttribute("OC", "0.1")]
        public string RepulsiveObstacleField
        {
            get { return _repulsiveObstacleField; }
            set {
                if (!value.Equals(_repulsiveObstacleField))
                {
                    _repulsiveObstacleField = value;
                    OnPropertyChanged("sf");
                }
            }
        }

        [EditBoxAttribute("OF", "0.3")]
        public string RepulsiveObstacleFactorField
        {
            get { return _repulsiveObstacleFactorField; }
            set {
                if (!value.Equals(_repulsiveObstacleFactorField))
                {
                    _repulsiveObstacleFactorField = value;
                    OnPropertyChanged("sf");
                }
            }
        }
        
        [SliderAttribute("Flow", 0, 50, 1)]
        public float Slider
        {
            get { return _slider; }
            set
            {
                if (!value.Equals(_slider))
                {
                    _slider = value;
                    OnPropertyChanged("flow");
                }
            }
        }

        [EditBoxAttribute("Simulation FPS", "10")]
        public string Fps
        {
            get { return _fps; }
            set
            {
                if (!value.Equals(_fps))
                {
                    _fps = value;
                    OnPropertyChanged("simfps");
                }
            }
        }

        [Checkbox("Show ID")]
        public bool AgentId
        {
            get { return _showAgentId; }
            set
            {
                if (!value.Equals(_showAgentId))
                {
                    _showAgentId = value;
                    OnPropertyChanged("agentid");
                }
            }
        }

        [Checkbox("Show Force (SF only)")]
        public bool ShowSocialForce
        {
            get { return _showSocialForce; }
            set
            {
                if (!value.Equals(_showSocialForce))
                {
                    _showSocialForce = value;
                    OnPropertyChanged("showsf");
                }
            }
        }

        [Button("Crowd measure tool")]
        public void CreatePolygon()
        {
            MainFrameUI.Tool.Option = MainFrameUI.Tool.ToolOption.DrawingPolygon;
        }

        [Button("Delete crowd measurment area")]
        public void DeletePolygon()
        {
            MainFrameUI.Tool.Option = MainFrameUI.Tool.ToolOption.DeletePolygon;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
