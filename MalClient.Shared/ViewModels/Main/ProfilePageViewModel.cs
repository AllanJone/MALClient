﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MalClient.Shared.Comm;
using MalClient.Shared.Comm.MagicalRawQueries.Profile;
using MalClient.Shared.Comm.Profile;
using MalClient.Shared.Delegates;
using MalClient.Shared.Models;
using MalClient.Shared.Models.Favourites;
using MalClient.Shared.Models.Library;
using MalClient.Shared.NavArgs;
using MalClient.Shared.Utils;
using MalClient.Shared.Utils.Enums;
using MalClient.Shared.ViewModels;

namespace MALClient.ViewModels.Main
{
    public sealed class ProfilePageViewModel : ViewModelBase
    {
        public event WebViewNavigationRequest OnWebViewNavigationRequest;
        public event EmptyEventHander OnInitialized;

        //anime -<>- manga
        private readonly Dictionary<string, Tuple<List<AnimeItemAbstraction>, List<AnimeItemAbstraction>>>
            _othersAbstractions =
                new Dictionary<string, Tuple<List<AnimeItemAbstraction>, List<AnimeItemAbstraction>>>();

        public Dictionary<string, Tuple<List<AnimeItemAbstraction>, List<AnimeItemAbstraction>>> OthersAbstractions
            => _othersAbstractions;

        private ObservableCollection<MalComment> _malComments = new ObservableCollection<MalComment>();

        public ObservableCollection<MalComment> MalComments
        {
            get { return _malComments; }
            set
            {
                _malComments = value;
                RaisePropertyChanged(() => MalComments);
            }
        }

        public List<FavouriteViewModel> _favouriteCharacters;

        public List<FavouriteViewModel> FavouriteCharacters
        {
            get { return _favouriteCharacters; }
            set
            {
                _favouriteCharacters = value;
                RaisePropertyChanged(() => FavouriteCharacters);
            }
        }

        public List<FavouriteViewModel> _favouriteStaff;

        public List<FavouriteViewModel> FavouriteStaff
        {
            get { return _favouriteStaff; }
            set
            {
                _favouriteStaff = value;
                RaisePropertyChanged(() => FavouriteStaff);
            }
        }

        private List<int> _animeChartValues = new List<int>();

        private string _currUser;
        public ProfilePageNavigationArgs PrevArgs;

        public async void LoadProfileData(ProfilePageNavigationArgs args, bool force = false)
        {
            if (args == null)
                args = PrevArgs;
            else
                PrevArgs = args;

            if (args == null)
                return;

            if (args.TargetUser == Credentials.UserName)
            {
                ViewModelLocator.NavMgr.ResetMainBackNav();
                if(ViewModelLocator.Mobile)
                    ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageAnimeList,null);
            }

            if (_currUser == null || _currUser != args.TargetUser || force)
            {
                LoadingVisibility = Visibility.Visible;
                await
                    Task.Run(
                        async () =>
                            CurrentData = await new ProfileQuery(false, args?.TargetUser ?? "").GetProfileData(force));
                _currUser = args.TargetUser ?? Credentials.UserName;
            }
            FavAnime = new List<AnimeItemViewModel>();
            FavManga = new List<AnimeItemViewModel>();
            RecentManga = new List<AnimeItemViewModel>();
            RecentAnime = new List<AnimeItemViewModel>();
            ViewModelLocator.GeneralMain.CurrentStatus = $"{_currUser} - Profile";
            var authenticatedUser = args.TargetUser.Equals(Credentials.UserName,StringComparison.CurrentCultureIgnoreCase);
            RaisePropertyChanged(() => CurrentData);
            LoadingVisibility = Visibility.Collapsed;
            RaisePropertyChanged(() => IsPinned);
            RaisePropertyChanged(() => PinProfileVisibility);
            MalComments = new ObservableCollection<MalComment>(CurrentData.Comments);
            FavouriteCharacters = new List<FavouriteViewModel>(CurrentData.FavouriteCharacters.Select(character => new FavouriteViewModel(character)));
            FavouriteStaff = new List<FavouriteViewModel>(CurrentData.FavouritePeople.Select(staff => new FavouriteViewModel(staff)));
            CommentInputBoxVisibility = string.IsNullOrEmpty(CurrentData.ProfileMemId) ? Visibility.Collapsed : Visibility.Visible; //posting restricted
            LoadAboutMeButtonVisibility = true;
            LoadingAboutMeVisibility = AboutMeWebViewVisibility = false;
            if (authenticatedUser)
            {
                _initialized = true;
                var list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.FavouriteAnime)
                {
                    var data = await ViewModelLocator.AnimeList.TryRetrieveAuthenticatedAnimeItem(id);

                    if (data != null)
                    {
                        list.Add((data as AnimeItemViewModel).ParentAbstraction.ViewModel);
                    }
                }
                FavAnime = list;
                list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.FavouriteManga)
                {
                    var data = await ViewModelLocator.AnimeList.TryRetrieveAuthenticatedAnimeItem(id, false);
                    if (data != null)
                    {
                        list.Add((data as AnimeItemViewModel).ParentAbstraction.ViewModel);
                    }
                }
                FavManga = list;
                list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.RecentAnime)
                {
                    var data = await ViewModelLocator.AnimeList.TryRetrieveAuthenticatedAnimeItem(id);
                    if (data != null)
                    {
                        list.Add((data as AnimeItemViewModel).ParentAbstraction.ViewModel);
                    }
                }
                RecentAnime = list;
                list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.RecentManga)
                {
                    var data = await ViewModelLocator.AnimeList.TryRetrieveAuthenticatedAnimeItem(id, false);
                    if (data != null)
                    {
                        list.Add((data as AnimeItemViewModel).ParentAbstraction.ViewModel);
                    }
                }
                RecentManga = list;
            }
            else
            {
                if (!_othersAbstractions.ContainsKey(args.TargetUser ?? ""))
                {
                    LoadingOhersLibrariesProgressVisiblity = Visibility.Visible;
                    var data = new List<ILibraryData>();
                    await
                        Task.Run(
                            async () =>
                                data =
                                    await
                                        new LibraryListQuery(args.TargetUser, AnimeListWorkModes.Anime).GetLibrary(false));

                    var abstractions = new List<AnimeItemAbstraction>();
                    foreach (var libraryData in data)
                        abstractions.Add(new AnimeItemAbstraction(false, libraryData as AnimeLibraryItemData));

                    await
                        Task.Run(
                            async () =>
                                data =
                                    await
                                        new LibraryListQuery(args.TargetUser, AnimeListWorkModes.Manga).GetLibrary(false));

                    var mangaAbstractions = new List<AnimeItemAbstraction>();
                    foreach (
                        var libraryData in data)
                        mangaAbstractions.Add(new AnimeItemAbstraction(false, libraryData as MangaLibraryItemData));

                    _othersAbstractions.Add(args.TargetUser,
                        new Tuple<List<AnimeItemAbstraction>, List<AnimeItemAbstraction>>(abstractions,
                            mangaAbstractions));

                    LoadingOhersLibrariesProgressVisiblity = Visibility.Collapsed;
                }

                var source = _othersAbstractions[args.TargetUser];
                var list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.FavouriteAnime)
                {
                    var data = source.Item1.FirstOrDefault(abs => abs.Id == id);

                    if (data != null)
                    {
                        list.Add(data.ViewModel);
                    }
                }
                FavAnime = list;
                list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.FavouriteManga)
                {
                    var data = source.Item2.FirstOrDefault(abs => abs.Id == id);

                    if (data != null)
                    {
                        list.Add(data.ViewModel);
                    }
                }
                FavManga = list;
                list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.RecentAnime)
                {
                    var data = source.Item1.FirstOrDefault(abs => abs.Id == id);

                    if (data != null)
                    {
                        list.Add(data.ViewModel);
                    }
                }
                RecentAnime = list;
                list = new List<AnimeItemViewModel>();
                foreach (var id in CurrentData.RecentManga)
                {
                    var data = source.Item2.FirstOrDefault(abs => abs.Id == id);

                    if (data != null)
                    {
                        list.Add(data.ViewModel);
                    }
                }
                RecentManga = list;
            }
            AnimeChartValues = new List<int>
            {
                CurrentData.AnimeWatching,
                CurrentData.AnimeCompleted,
                CurrentData.AnimeOnHold,
                CurrentData.AnimeDropped,
                CurrentData.AnimePlanned
            };
            MangaChartValues = new List<int>
            {
                CurrentData.MangaReading,
                CurrentData.MangaCompleted,
                CurrentData.MangaOnHold,
                CurrentData.MangaDropped,
                CurrentData.MangaPlanned
            };

            EmptyRecentAnimeNoticeVisibility = RecentAnime.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            EmptyRecentMangaNoticeVisibility = RecentManga.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            EmptyFavCharactersNoticeVisibility = CurrentData.FavouriteCharacters.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            EmptyFavAnimeNoticeVisibility = FavAnime.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            EmptyFavMangaNoticeVisibility = FavManga.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            EmptyFavPeopleNoticeVisibility = CurrentData.FavouritePeople.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            OnInitialized?.Invoke();
        }

        private void NavigateDetails(AnimeCharacter character)
        {
            ViewModelLocator.GeneralMain.Navigate(PageIndex.PageAnimeDetails,
                new AnimeDetailsPageNavigationArgs(int.Parse(character.ShowId), character.Notes, null,
                    null)
                {
                    Source = PageIndex.PageProfile,
                    AnimeMode = character.FromAnime
                });
        }

        private async void NavigateCharacterWebPage(AnimeCharacter character)
        {
            await Launcher.LaunchUriAsync(new Uri($"http://myanimelist.net/character/{character.Id}"));
        }

        private async void NavigatePersonWebPage(AnimeStaffPerson person)
        {
            await Launcher.LaunchUriAsync(new Uri($"http://myanimelist.net/people/{person.Id}"));
        }

        #region Props

        private Visibility _emptyFavAnimeNoticeVisibility = Visibility.Collapsed;

        private Visibility _emptyFavCharactersNoticeVisibility = Visibility.Collapsed;

        private Visibility _emptyFavMangaNoticeVisibility = Visibility.Collapsed;

        private Visibility _emptyFavPeopleNoticeVisibility = Visibility.Collapsed;

        private Visibility _emptyRecentAnimeNoticeVisibility = Visibility.Collapsed;

        private Visibility _emptyRecentMangaNoticeVisibility = Visibility.Collapsed;
        private List<AnimeItemViewModel> _favAnime;
        private List<AnimeItemViewModel> _favManga;

        private bool _initialized;


        private Visibility _loadingVisibility = Visibility.Collapsed;

        private List<int> _mangaChartValues = new List<int>();

        private ICommand _navigateCharPageCommand;

        private ICommand _navigateDetailsCommand;

        private ICommand _navigatePersonPageCommand;

        private ICommand _navAnimeListCommand;

        private ICommand _navMangaListCommand;

        private ICommand _navigateHistoryCommand;

        public ICommand NavigateHistoryCommand
            =>
                _navigateHistoryCommand ??
                (_navigateHistoryCommand =
                    new RelayCommand(
                        () =>
                        {

                            ViewModelLocator.GeneralMain.Navigate(PageIndex.PageHistory,
                                new HistoryNavigationArgs {Source = CurrentData.User.Name});
                            ViewModelLocator.NavMgr.RegisterOneTimeMainOverride(
                                new RelayCommand(
                                    () =>
                                    {
                                        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageProfile,
                                            new ProfilePageNavigationArgs {TargetUser = CurrentData.User.Name});
                                    }));

                        }));

        private ICommand _sendCommentCommand;

        public ICommand SendCommentCommand => _sendCommentCommand ?? (_sendCommentCommand = new RelayCommand(async () =>
        {
            if(string.IsNullOrEmpty(CommentText))
                return;
            IsSendCommentButtonEnabled = false;
            if (await
                ProfileCommentQueries.SendComment(CurrentData.User?.Name ?? Credentials.UserName,
                    CurrentData.ProfileMemId,
                    CommentText))
            {
                CommentText = "";
                await CurrentData.UpdateComments();
                MalComments = new ObservableCollection<MalComment>(CurrentData.Comments);
            }
            IsSendCommentButtonEnabled = true;
        }));

        private ICommand _deleteCommentCommand;

        public ICommand DeleteCommentCommand => _deleteCommentCommand ?? (_deleteCommentCommand = new RelayCommand<MalComment>(async comment =>
        {
            if (await ProfileCommentQueries.DeleteComment(comment.Id))
            {
                MalComments.Remove(comment);
                var data = CurrentData;
                data.Comments = MalComments.ToList();
                DataCache.SaveProfileData(_currUser, data);
            }
        }));

        private ICommand _navigateConversationCommand;

        public ICommand NavigateConversationCommand => _navigateConversationCommand ?? (_navigateConversationCommand = new RelayCommand<MalComment>(comment =>
        {
            ViewModelLocator.GeneralMain.Navigate(PageIndex.PageMessageDetails,
                new MalMessageDetailsNavArgs {WorkMode = MessageDetailsWorkMode.ProfileComments, Arg = comment});
        }));

        private bool _refreshingComments;
        private ICommand _refreshCommentsCommand;

        public ICommand RefreshCommentsCommand => _refreshCommentsCommand ?? (_refreshCommentsCommand = new RelayCommand(async () =>
        {
            if(_refreshingComments)
                return;
            LoadingCommentsVisiblity = Visibility.Visible;
            _refreshingComments = true;
            await CurrentData.UpdateComments();
            MalComments = new ObservableCollection<MalComment>(CurrentData.Comments);
            _refreshingComments = false;
            LoadingCommentsVisiblity = Visibility.Collapsed;
        }));

        private List<AnimeItemViewModel> _recentAnime;
        private List<AnimeItemViewModel> _recentManga;

        public ProfilePageViewModel()
        {
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            MaxWidth = bounds.Width/2.2;
        }

        public ProfileData CurrentData { get; set; } = new ProfileData();

        public List<AnimeItemViewModel> RecentAnime
        {
            get { return _recentAnime; }
            private set
            {
                _recentAnime = value;
                RaisePropertyChanged(() => RecentAnime);
            }
        }

        public List<AnimeItemViewModel> RecentManga
        {
            get { return _recentManga; }
            private set
            {
                _recentManga = value;
                RaisePropertyChanged(() => RecentManga);
            }
        }

        public List<AnimeItemViewModel> FavAnime
        {
            get { return _favAnime; }
            private set
            {
                _favAnime = value;
                RaisePropertyChanged(() => FavAnime);
            }
        }

        public List<AnimeItemViewModel> FavManga
        {
            get { return _favManga; }
            private set
            {
                _favManga = value;
                RaisePropertyChanged(() => FavManga);
            }
        }

        public AnimeItemViewModel TemporarilySelectedAnimeItem
        {
            get { return null; }
            set { value?.NavigateDetails(PageIndex.PageProfile); }
        }

        public Visibility LoadingVisibility
        {
            get { return _loadingVisibility; }
            set
            {
                _loadingVisibility = value;
                RaisePropertyChanged(() => LoadingVisibility);
            }
        }

        private Visibility _authenticatedControlsVisibility;

        public Visibility AuthenticatedControlsVisibility
        {
            get { return _authenticatedControlsVisibility; }
            set
            {
                _authenticatedControlsVisibility = value;
                RaisePropertyChanged(() => AuthenticatedControlsVisibility);
            }
        }

        private Visibility _commentInputBoxVisibility;

        public Visibility CommentInputBoxVisibility
        {
            get { return _commentInputBoxVisibility; }
            set
            {
                _commentInputBoxVisibility = value;
                RaisePropertyChanged(() => CommentInputBoxVisibility);
            }
        }

        public List<int> AnimeChartValues
        {
            get { return _animeChartValues; }
            set
            {
                _animeChartValues = value;
                RaisePropertyChanged(() => AnimeChartValues);
            }
        }

        public List<int> MangaChartValues
        {
            get { return _mangaChartValues; }
            set
            {
                _mangaChartValues = value;
                RaisePropertyChanged(() => MangaChartValues);
            }
        }

        public bool IsPinned
        {
            get { return CurrentData.User.Name != null && Settings.PinnedProfiles.Contains(CurrentData.User.Name); }
            set
            {
                if (value)
                    Settings.PinnedProfiles += ";" + CurrentData.User.Name;
                else
                {
                    var pinned = Settings.PinnedProfiles.Split(';').ToList();
                    pinned.Remove(CurrentData.User.Name);
                    Settings.PinnedProfiles = string.Join(";",pinned);
                }
                ViewModelLocator.GeneralHamburger.UpdatePinnedProfiles();
                RaisePropertyChanged(() => IsPinned);
            }
        }

        public static double MaxWidth { get; set; }

        public ICommand NavigateDetailsCommand
            => _navigateDetailsCommand ?? (_navigateDetailsCommand = new RelayCommand<AnimeCharacter>(NavigateDetails));

        public ICommand NavigateCharPageCommand
            =>
                _navigateCharPageCommand ??
                (_navigateCharPageCommand = new RelayCommand<AnimeCharacter>(NavigateCharacterWebPage));

        public ICommand NavigatePersonPageCommand
            =>
                _navigatePersonPageCommand ??
                (_navigatePersonPageCommand = new RelayCommand<AnimeStaffPerson>(NavigatePersonWebPage));

        public ICommand NavigateAnimeListCommand
            =>
                _navAnimeListCommand ??
                (_navAnimeListCommand = new RelayCommand(() =>
                {
                    ViewModelLocator.NavMgr.RegisterOneTimeMainOverride(
                        new RelayCommand(
                            () =>
                            {
                                ViewModelLocator.GeneralMain.Navigate(PageIndex.PageProfile,
                                    new ProfilePageNavigationArgs{TargetUser = CurrentData.User.Name});
                            }));
                    ViewModelLocator.GeneralMain.Navigate(PageIndex.PageAnimeList,
                        new AnimeListPageNavigationArgs(0, AnimeListWorkModes.Anime) {ListSource = _currUser, ResetBackNav = false });
                }));

        public ICommand NavigateMangaListCommand
            =>
                _navMangaListCommand ??
                (_navMangaListCommand =
                    new RelayCommand(
                        () =>
                        {
                            ViewModelLocator.NavMgr.RegisterOneTimeMainOverride(
                                new RelayCommand(
                                    () =>
                                    {
                                        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageProfile,
                                            new ProfilePageNavigationArgs {TargetUser = CurrentData.User.Name});
                                    }));
                            ViewModelLocator.GeneralMain.Navigate(PageIndex.PageAnimeList,
                                new AnimeListPageNavigationArgs(0, AnimeListWorkModes.Manga)
                                {
                                    ListSource = _currUser,
                                    ResetBackNav = false
                                });
                        }))
            ;

        public ICommand LoadAboutMeCommand => _loadAboutMeCommand ?? (_loadAboutMeCommand = new RelayCommand(() =>
        {
            LoadAboutMeButtonVisibility = false;           
            if (!string.IsNullOrEmpty(CurrentData.HtmlContent))
            {
                LoadingAboutMeVisibility = true;
                OnWebViewNavigationRequest?.Invoke(CurrentData.HtmlContent, false);
            }
            
        }));


        public Visibility PinProfileVisibility
            => CurrentData.User.Name == null || Credentials.UserName.Equals(CurrentData.User.Name,StringComparison.CurrentCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;


        public Visibility EmptyFavAnimeNoticeVisibility
        {
            get { return _emptyFavAnimeNoticeVisibility; }
            set
            {
                _emptyFavAnimeNoticeVisibility = value;
                RaisePropertyChanged(() => EmptyFavAnimeNoticeVisibility);
            }
        }

        public Visibility EmptyFavCharactersNoticeVisibility
        {
            get { return _emptyFavCharactersNoticeVisibility; }
            set
            {
                _emptyFavCharactersNoticeVisibility = value;
                RaisePropertyChanged(() => EmptyFavCharactersNoticeVisibility);
            }
        }

        public Visibility EmptyFavMangaNoticeVisibility
        {
            get { return _emptyFavMangaNoticeVisibility; }
            set
            {
                _emptyFavMangaNoticeVisibility = value;
                RaisePropertyChanged(() => EmptyFavMangaNoticeVisibility);
            }
        }

        public Visibility EmptyRecentMangaNoticeVisibility
        {
            get { return _emptyRecentMangaNoticeVisibility; }
            set
            {
                _emptyRecentMangaNoticeVisibility = value;
                RaisePropertyChanged(() => EmptyRecentMangaNoticeVisibility);
            }
        }

        public Visibility EmptyRecentAnimeNoticeVisibility
        {
            get { return _emptyRecentAnimeNoticeVisibility; }
            set
            {
                _emptyRecentAnimeNoticeVisibility = value;
                RaisePropertyChanged(() => EmptyRecentAnimeNoticeVisibility);
            }
        }

        public Visibility EmptyFavPeopleNoticeVisibility
        {
            get { return _emptyFavPeopleNoticeVisibility; }
            set
            {
                _emptyFavPeopleNoticeVisibility = value;
                RaisePropertyChanged(() => EmptyFavPeopleNoticeVisibility);
            }
        }

        private Visibility _loadingOhersLibrariesProgressVisiblity = Visibility.Collapsed;

        public Visibility LoadingOhersLibrariesProgressVisiblity
        {
            get { return _loadingOhersLibrariesProgressVisiblity; }
            set
            {
                _loadingOhersLibrariesProgressVisiblity = value;
                RaisePropertyChanged(() => LoadingOhersLibrariesProgressVisiblity);
            }
        }

        private Visibility _loadingCommentsVisiblity = Visibility.Collapsed;

        public Visibility LoadingCommentsVisiblity
        {
            get { return _loadingCommentsVisiblity; }
            set
            {
                _loadingCommentsVisiblity = value;
                RaisePropertyChanged(() => LoadingCommentsVisiblity);
            }
        }

        private bool _loadAboutMeButtonVisibility = true;

        public bool LoadAboutMeButtonVisibility
        {
            get { return _loadAboutMeButtonVisibility; }
            set
            {
                _loadAboutMeButtonVisibility = value;
                RaisePropertyChanged(() => LoadAboutMeButtonVisibility);
            }
        }

        private bool _loadingAboutMeVisibility = false;

        public bool LoadingAboutMeVisibility
        {
            get { return _loadingAboutMeVisibility; }
            set
            {
                _loadingAboutMeVisibility = value;
                RaisePropertyChanged(() => LoadingAboutMeVisibility);
            }
        }

        private bool _aboutMeWebViewVisibility = false;

        public bool AboutMeWebViewVisibility
        {
            get { return _aboutMeWebViewVisibility; }
            set
            {
                _aboutMeWebViewVisibility = value;
                RaisePropertyChanged(() => AboutMeWebViewVisibility);
            }
        }

        private string _commentText;

        public string CommentText
        {
            get { return _commentText; }
            set
            {
                _commentText = value;
                RaisePropertyChanged(() => CommentText);
            }
        }

        private bool _isSendCommentButtonEnabled = true;

        private ICommand _loadAboutMeCommand;
        private ICommand _navigateCharacterDetailsCommand;
        private ICommand _navigateStaffDetailsCommand;
        private ICommand _toggleAboutMeWebViewVisibilityCommand;

        public bool IsSendCommentButtonEnabled
        {
            get { return _isSendCommentButtonEnabled; }
            set
            {
                _isSendCommentButtonEnabled = value;
                RaisePropertyChanged(() => IsSendCommentButtonEnabled);
            }
        }

        public ICommand NavigateCharacterDetailsCommand
            =>
                _navigateCharacterDetailsCommand ??
                (_navigateCharacterDetailsCommand =
                    new RelayCommand<AnimeCharacter>(
                        entry =>
                        {
                            if (ViewModelLocator.Mobile)
                                ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageProfile, PrevArgs);
                            else if (ViewModelLocator.GeneralMain.OffContentVisibility == Visibility.Visible)
                            {
                                if (ViewModelLocator.GeneralMain.CurrentOffPage == PageIndex.PageStaffDetails)
                                    ViewModelLocator.StaffDetails.RegisterSelfBackNav(int.Parse(entry.Id));
                                else if (ViewModelLocator.GeneralMain.CurrentOffPage == PageIndex.PageCharacterDetails)
                                    ViewModelLocator.CharacterDetails.RegisterSelfBackNav(int.Parse(entry.Id));
                            }
                            ViewModelLocator.GeneralMain.Navigate(PageIndex.PageCharacterDetails,
                                new CharacterDetailsNavigationArgs {Id = int.Parse(entry.Id)});
                        }));

        public ICommand NavigateStaffDetailsCommand
            =>
                _navigateStaffDetailsCommand ??
                (_navigateStaffDetailsCommand =
                    new RelayCommand<FavouriteBase>(
                        entry =>
                        {
                            if (ViewModelLocator.Mobile)
                                ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageProfile, PrevArgs);
                            else if (ViewModelLocator.GeneralMain.OffContentVisibility == Visibility.Visible)
                            {
                                if (ViewModelLocator.GeneralMain.CurrentOffPage == PageIndex.PageStaffDetails)
                                    ViewModelLocator.StaffDetails.RegisterSelfBackNav(int.Parse(entry.Id));
                                else if (ViewModelLocator.GeneralMain.CurrentOffPage == PageIndex.PageCharacterDetails)
                                    ViewModelLocator.CharacterDetails.RegisterSelfBackNav(int.Parse(entry.Id));
                            }
                            ViewModelLocator.GeneralMain.Navigate(PageIndex.PageStaffDetails,
                                new StaffDetailsNaviagtionArgs {Id = int.Parse(entry.Id)});
                        }));

        public ICommand ToggleAboutMeWebViewVisibilityCommand
            =>
                _toggleAboutMeWebViewVisibilityCommand ??
                (_toggleAboutMeWebViewVisibilityCommand =
                    new RelayCommand<FavouriteBase>(
                        entry =>
                        {
                            AboutMeWebViewVisibility = !AboutMeWebViewVisibility;
                        }));

        #endregion
    }
}