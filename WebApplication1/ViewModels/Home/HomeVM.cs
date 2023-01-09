using WebApplication1.Models;

namespace WebApplication1.ViewModels.Home
{
    public class HomeVM
    {
        public IEnumerable<Slider> Sliders { get; set; }
        public IEnumerable<Sponsor> Sponsors { get; set; }
    }
}
