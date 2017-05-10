using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using WinRTXamlToolkit.Controls.Extensions;

namespace MALClient.UWP.UserControls
{
    public class BlurHelper
    {
        private readonly FrameworkElement _element;
        Compositor _compositor;
        SpriteVisual _hostSprite;

        public BlurHelper(FrameworkElement element)
        {
            _element = element;
            Blurify();
        }

        public BlurHelper(CommandBar element)
        {
            _element = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            var background = (element.GetDescendants().First() as Panel);
            background.Background = new SolidColorBrush(Color.FromArgb(0x7f,0,0,0));
            background.Children.Add(_element);
            Blurify();
        }

        private void Blurify()
        {
            try
            {
                _compositor = ElementCompositionPreview.GetElementVisual(_element).Compositor;
                _hostSprite = _compositor.CreateSpriteVisual();
                _hostSprite.Size = new Vector2((float)_element.ActualWidth, (float)_element.ActualHeight);

                ElementCompositionPreview.SetElementChildVisual(_element, _hostSprite);
                _hostSprite.Brush = _compositor.CreateHostBackdropBrush();
            }
            catch (Exception)
            {
                //not CU
            }
        }
        
    }
}
