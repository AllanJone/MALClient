using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MALClient.Android.Activities;
using MALClient.Android.BindingInformation;
using MALClient.Android.CollectionAdapters;
using MALClient.Android.Resources;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Android.Fragments.CalendarFragments
{
    public class CalendarPageTabFragment : MalFragmentBase
    {
        private readonly List<AnimeItemViewModel> _items;
        private GridViewColumnHelper _gridViewColumnHelper;

        public CalendarPageTabFragment(List<AnimeItemViewModel> items)
        {
            _items = items;
        }

        protected override void Init(Bundle savedInstanceState)
        {

        }

        protected override void InitBindings()
        {
            //todo binding info
            //CalendarPageTabContentList.Adapter = new AnimeListItemsAdapter(MainActivity.CurrentContext,
            //    Resource.Layout.AnimeGridItem, _items, (model, view,fling) => new AnimeGridItemBindingInfo(view, model,fling, false));
            _gridViewColumnHelper = new GridViewColumnHelper(CalendarPageTabContentList);
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            _gridViewColumnHelper.OnConfigurationChanged(newConfig);
            base.OnConfigurationChanged(newConfig);
        }

        public override int LayoutResourceId => Resource.Layout.CalenarPageTabContent;

        #region Views

        private GridView _calendarPageTabContentList;

        public GridView CalendarPageTabContentList => _calendarPageTabContentList ?? (_calendarPageTabContentList = FindViewById<GridView>(Resource.Id.CalendarPageTabContentList));

        #endregion
    }
}