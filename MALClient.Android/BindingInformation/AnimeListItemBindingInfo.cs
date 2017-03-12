using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Shehabic.Droppy;
using FFImageLoading;
using FFImageLoading.Views;
using GalaSoft.MvvmLight.Command;
using MALClient.Android.CollectionAdapters;
using MALClient.XShared.ViewModels;
using GalaSoft.MvvmLight.Helpers;
using MALClient.Android.Activities;
using MALClient.Android.BindingConverters;
using MALClient.Android.DIalogs;
using MALClient.Android.Flyouts;
using MALClient.Android.Listeners;
using MALClient.Android.Resources;
using Org.Apache.Http.Conn;

namespace MALClient.Android.BindingInformation
{
    class AnimeListItemBindingInfo : BindingInfo<AnimeItemViewModel>
    {
        private DroppyMenuPopup _menu;

        public Action<AnimeItemViewModel> OnItemClickAction { get; set; }

        public AnimeListItemBindingInfo(View container, AnimeItemViewModel viewModel,bool fling) : base(container, viewModel,fling)
        {
            PrepareContainer();
        }

        protected override void InitBindings()
        {
            if(Fling)
                return;

            
            Bindings.Add(new Binding<string, string>(
                ViewModel,
                () => ViewModel.MyEpisodesBind,
                AnimeListItemWatchedButton,
                () => AnimeListItemWatchedButton.Text));

            
            Bindings.Add(new Binding<string, string>(
                ViewModel,
                () => ViewModel.MyScoreBind,
                AnimeListItemScoreButton,
                () => AnimeListItemScoreButton.Text));

            
            Bindings.Add(new Binding<string, string>(
                ViewModel,
                () => ViewModel.MyStatusBind,
                AnimeListItemStatusButton,
                () => AnimeListItemStatusButton.Text));

            
            Bindings.Add(new Binding<bool, ViewStates>(
                ViewModel,
                () => ViewModel.IncrementEpsVisibility,
                AnimeListItemIncButton,
                () => AnimeListItemIncButton.Visibility).ConvertSourceToTarget(Converters.BoolToVisibility));

            
            Bindings.Add(new Binding<bool, ViewStates>(
                ViewModel,
                () => ViewModel.DecrementEpsVisibility,
                AnimeListItemDecButton,
                () => AnimeListItemDecButton.Visibility).ConvertSourceToTarget(Converters.BoolToVisibility));

            
            Bindings.Add(new Binding<bool, ViewStates>(
                ViewModel,
                () => ViewModel.Auth,
                AnimeListItemStatusScoreSection,
                () => AnimeListItemStatusScoreSection.Visibility).ConvertSourceToTarget(Converters.BoolToVisibility));



            AnimeListItemMoreButton.SetOnClickListener(new OnClickListener(view => AnimeListItemMoreButtonOnClick()));
            AnimeListItemWatchedButton.SetCommand("Click",new RelayCommand(ShowWatchedDialog));
            AnimeListItemStatusButton.SetCommand("Click",new RelayCommand(ShowStatusDialog));
            AnimeListItemScoreButton.SetCommand("Click",new RelayCommand(ShowRatingDialog));

            AnimeListItemIncButton.SetCommand("Click",ViewModel.IncrementWatchedCommand);
            AnimeListItemDecButton.SetCommand("Click",ViewModel.DecrementWatchedCommand);
        }

        private void ContainerOnClick()
        {
            if (OnItemClickAction != null)
                OnItemClickAction.Invoke(ViewModel);
            else
                ViewModel.NavigateDetailsCommand.Execute(null);
        }


        private void AnimeListItemMoreButtonOnClick()
        {
            _menu = AnimeItemFlyoutBuilder.BuildForAnimeItem(MainActivity.CurrentContext, AnimeListItemMoreButton, null,
                OnMoreFlyoutSelection, true);
            _menu.Show();
        }

        private void OnMoreFlyoutSelection(AnimeGridItemMoreFlyoutButtons animeGridItemMoreFlyoutButtons)
        {
            switch (animeGridItemMoreFlyoutButtons)
            {
                case AnimeGridItemMoreFlyoutButtons.CopyLink:
                    ViewModel.CopyLinkToClipboardCommand.Execute(null);
                    break;
                case AnimeGridItemMoreFlyoutButtons.OpenInBrowser:
                    ViewModel.OpenInMALCommand.Execute(null);
                    break;
            }
            _menu.Dismiss(true);
        }

        #region Dialogs

        private void ShowStatusDialog()
        {
            AnimeUpdateDialogBuilder.BuildStatusDialog(ViewModel, ViewModel.ParentAbstraction.RepresentsAnime);
        }
        private void ShowWatchedDialog()
        {
            AnimeUpdateDialogBuilder.BuildWatchedDialog(ViewModel);
        }
        private void ShowRatingDialog()
        {
            AnimeUpdateDialogBuilder.BuildScoreDialog(ViewModel);
        }
        #endregion

        protected override void InitOneTimeBindings()
        {
            AnimeListItemTitle.Text = ViewModel.Title;


            if (!Fling && (int)Container.Tag != ViewModel.Id)
            {
                AnimeListItemImgPlaceholder.Visibility = ViewStates.Gone;
                AnimeListItemImage.AnimeInto(ViewModel.ImgUrl);

                Container.SetOnClickListener(new OnClickListener(view => ContainerOnClick()));

                ViewModel.AnimeItemDisplayContext = ViewModelLocator.AnimeList.AnimeItemsDisplayContext;

                Container.Tag = ViewModel.Id;
            }
            else if(Fling)
            {
                AnimeListItemImage.Visibility = ViewStates.Invisible;
                AnimeListItemImgPlaceholder.Visibility = ViewStates.Visible;
            }
        }

        protected override void DetachInnerBindings()
        {
            if (Bindings.Any())
            {
                Container.SetOnClickListener(null);
            }
        }

        #region Views

        private ImageView _animeListItemImgPlaceholder;
        private ImageViewAsync _animeListItemImage;
        private ImageButton _animeListItemMoreButton;
        private TextView _animeListItemTitle;
        private TextView _animeGridItemToLeftInfo;
        private TextView _animeListItemTypeTextView;
        private Button _animeListItemWatchedButton;
        private LinearLayout _animeListItemBtmRightSectionTop;
        private Button _animeListItemStatusButton;
        private Button _animeListItemScoreButton;
        private LinearLayout _animeListItemStatusScoreSection;
        private ImageButton _animeListItemIncButton;
        private ImageButton _animeListItemDecButton;
        private LinearLayout _animeListItemIncDecSection;

        public ImageView AnimeListItemImgPlaceholder => _animeListItemImgPlaceholder ?? (_animeListItemImgPlaceholder = Container.FindViewById<ImageView>(Resource.Id.AnimeListItemImgPlaceholder));

        public ImageViewAsync AnimeListItemImage => _animeListItemImage ?? (_animeListItemImage = Container.FindViewById<ImageViewAsync>(Resource.Id.AnimeListItemImage));

        public ImageButton AnimeListItemMoreButton => _animeListItemMoreButton ?? (_animeListItemMoreButton = Container.FindViewById<ImageButton>(Resource.Id.AnimeListItemMoreButton));

        public TextView AnimeListItemTitle => _animeListItemTitle ?? (_animeListItemTitle = Container.FindViewById<TextView>(Resource.Id.AnimeListItemTitle));

        public TextView AnimeGridItemToLeftInfo => _animeGridItemToLeftInfo ?? (_animeGridItemToLeftInfo = Container.FindViewById<TextView>(Resource.Id.AnimeGridItemToLeftInfo));

        public TextView AnimeListItemTypeTextView => _animeListItemTypeTextView ?? (_animeListItemTypeTextView = Container.FindViewById<TextView>(Resource.Id.AnimeListItemTypeTextView));

        public Button AnimeListItemWatchedButton => _animeListItemWatchedButton ?? (_animeListItemWatchedButton = Container.FindViewById<Button>(Resource.Id.AnimeListItemWatchedButton));

        public LinearLayout AnimeListItemBtmRightSectionTop => _animeListItemBtmRightSectionTop ?? (_animeListItemBtmRightSectionTop = Container.FindViewById<LinearLayout>(Resource.Id.AnimeListItemBtmRightSectionTop));

        public Button AnimeListItemStatusButton => _animeListItemStatusButton ?? (_animeListItemStatusButton = Container.FindViewById<Button>(Resource.Id.AnimeListItemStatusButton));

        public Button AnimeListItemScoreButton => _animeListItemScoreButton ?? (_animeListItemScoreButton = Container.FindViewById<Button>(Resource.Id.AnimeListItemScoreButton));

        public LinearLayout AnimeListItemStatusScoreSection => _animeListItemStatusScoreSection ?? (_animeListItemStatusScoreSection = Container.FindViewById<LinearLayout>(Resource.Id.AnimeListItemStatusScoreSection));

        public ImageButton AnimeListItemIncButton => _animeListItemIncButton ?? (_animeListItemIncButton = Container.FindViewById<ImageButton>(Resource.Id.AnimeListItemIncButton));

        public ImageButton AnimeListItemDecButton => _animeListItemDecButton ?? (_animeListItemDecButton = Container.FindViewById<ImageButton>(Resource.Id.AnimeListItemDecButton));

        public LinearLayout AnimeListItemIncDecSection => _animeListItemIncDecSection ?? (_animeListItemIncDecSection = Container.FindViewById<LinearLayout>(Resource.Id.AnimeListItemIncDecSection));



        #endregion
    }
}