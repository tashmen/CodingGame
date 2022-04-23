using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralTrainer
{
    public class GameResultDto
    {
        public Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> outputs = new Dictionary<string, List<string>>();
        public List<string> summaries = new List<string>();
        public List<string> views = new List<string>();
        public Dictionary<int, int> scores = new Dictionary<int, int>();
        public List<string> uinput = new List<string>();
        public string metadata;
        //public List<TooltipDto> tooltips = new List<TooltipDto>();
        public Dictionary<int, int> ids = new Dictionary<int, int>();
        //public List<AgentDto> agents = new List<AgentDto>();
        public String failCause = null;
    }
}
