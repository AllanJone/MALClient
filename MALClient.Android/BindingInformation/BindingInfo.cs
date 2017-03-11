using System.Collections.Generic;
using System.Linq;
using Android.Views;
using GalaSoft.MvvmLight.Helpers;

namespace MALClient.Android.BindingInformation
{
    public abstract class BindingInfo<TViewModel>
    {
        private View _container;

        private bool _initialized;
        protected abstract void InitBindings();
        protected abstract void InitOneTimeBindings();
        protected abstract void DetachInnerBindings();
        protected List<Binding> Bindings { get; set; } = new List<Binding>();

        protected TViewModel ViewModel { get; private set; }
        public bool Fling { get; set; }
        public virtual int Position { get; set; }

        public View Container
        {
            get { return _container; }
            set
            {
                
                Detach();
                _container = value;
                if(_initialized)
                    PrepareContainer();
            }
        }

        protected BindingInfo(View container, TViewModel viewModel,bool fling)
        {          
            ViewModel = viewModel;
            Fling = fling;
            Container = container;
            _initialized = true;
        }

        protected void PrepareContainer()
        {
            InitOneTimeBindings();
            InitBindings();
        }

        private void DetachBaseBindings()
        {
            foreach (var binding in Bindings)
            {
                binding?.Detach();
            }
            Bindings = new List<Binding>();
        }

        public void Detach()
        {
            DetachBaseBindings();
            DetachInnerBindings();
        }
    }
}