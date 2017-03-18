using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Mikepenz.Materialdrawer;
using Com.Mikepenz.Materialdrawer.Model;
using Com.Mikepenz.Materialdrawer.Model.Interfaces;
using FFImageLoading;
using MALClient.Android.Resources;
using MALClient.Models.Enums;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.Interfaces;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;
using static MALClient.Android.HamburgerUtilities;

namespace MALClient.Android.Activities
{
    public partial class MainActivity : IHamburgerViewModel
    {
        private object GetAppropriateArgsForPage(PageIndex page)
        {
            switch (page)
            {
                case PageIndex.PageSeasonal:
                    return AnimeListPageNavigationArgs.Seasonal;
                case PageIndex.PageMangaList:
                    return AnimeListPageNavigationArgs.Manga;
                case PageIndex.PageMangaSearch:
                    return new SearchPageNavigationArgs { Anime = false };
                case PageIndex.PageSearch:
                    return new SearchPageNavigationArgs();
                case PageIndex.PageTopAnime:
                    return AnimeListPageNavigationArgs.TopAnime(TopAnimeType.General);
                case PageIndex.PageTopManga:
                    return AnimeListPageNavigationArgs.TopManga;
                case PageIndex.PageArticles:
                    return MalArticlesPageNavigationArgs.Articles;
                case PageIndex.PageNews:
                    return MalArticlesPageNavigationArgs.News;
                case PageIndex.PageProfile:
                    return new ProfilePageNavigationArgs { TargetUser = Credentials.UserName };
                case PageIndex.PageWallpapers:
                    return new WallpaperPageNavigationArgs();
                default:
                    return null;
            }
        }



        private async void BuildDrawer()
        {
            var builder = new DrawerBuilder().WithActivity(this);
            builder.WithSliderBackgroundColorRes(ResourceExtension.BrushHamburgerBackgroundRes);
            builder.WithStickyFooterShadow(true);
            builder.WithDisplayBelowStatusBar(true);


            var animeButton = GetBasePrimaryItem();
            animeButton.WithName("Anime list");
            animeButton.WithIdentifier((int)PageIndex.PageAnimeList);
            animeButton.WithIcon(Resource.Drawable.icon_list);

            var searchButton = GetBasePrimaryItem();
            searchButton.WithName("Anime search");
            searchButton.WithIdentifier((int)PageIndex.PageSearch);
            searchButton.WithIcon(Resource.Drawable.icon_search);

            var seasonalButton = GetBasePrimaryItem();
            seasonalButton.WithName("Seasonal anime");
            seasonalButton.WithIdentifier((int)PageIndex.PageSeasonal);
            seasonalButton.WithIcon(Resource.Drawable.icon_seasonal);

            var recomButton = GetBasePrimaryItem();
            recomButton.WithName("Recommendations");
            recomButton.WithIdentifier((int)PageIndex.PageRecomendations);
            recomButton.WithIcon(Resource.Drawable.icon_recom);

            var topAnimeButton = GetBasePrimaryItem();
            topAnimeButton.WithName("Top anime");
            topAnimeButton.WithIdentifier((int)PageIndex.PageTopAnime);
            topAnimeButton.WithIcon(Resource.Drawable.icon_fav_outline);

            var calendarButton = GetBasePrimaryItem();
            calendarButton.WithName("Calendar");
            calendarButton.WithIdentifier((int)PageIndex.PageCalendar);
            calendarButton.WithIcon(Resource.Drawable.icon_calendar);

            //

            var mangaListButton = GetBaseSecondaryItem();
            mangaListButton.WithName("Manga list");
            mangaListButton.WithIdentifier((int)PageIndex.PageMangaList);
            mangaListButton.WithIcon(Resource.Drawable.icon_books);

            var topMangaButton = GetBaseSecondaryItem();
            topMangaButton.WithName("Top manga");
            topMangaButton.WithIdentifier((int)PageIndex.PageTopManga);
            topMangaButton.WithIcon(Resource.Drawable.icon_fav_outline);

            var articlesButton = GetBaseSecondaryItem();
            articlesButton.WithName("Articles & News");
            articlesButton.WithIdentifier((int) PageIndex.PageArticles);
            articlesButton.WithIcon(Resource.Drawable.icon_newspaper);

            var videoButton = GetBaseSecondaryItem();
            videoButton.WithName("Promotional Videos");
            videoButton.WithIdentifier((int) PageIndex.PagePopularVideos);
            videoButton.WithIcon(Resource.Drawable.icon_video);

            var feedsButton = GetBaseSecondaryItem();
            feedsButton.WithName("Friends Feeds");
            feedsButton.WithIdentifier((int) PageIndex.PageFeeds);
            feedsButton.WithIcon(Resource.Drawable.icon_feeds);

            var forumsButton = GetBaseSecondaryItem();
            forumsButton.WithName("Forums");
            forumsButton.WithIdentifier((int) PageIndex.PageForumIndex);
            forumsButton.WithIcon(Resource.Drawable.icon_forum);

            var messagingButton = GetBaseSecondaryItem();
            messagingButton.WithName("Messaging");
            messagingButton.WithIdentifier((int) PageIndex.PageMessanging);
            messagingButton.WithIcon(Resource.Drawable.icon_message_alt);

            //

            var mangaSubHeader = new SectionDrawerItem();
            mangaSubHeader.WithName("Manga");
            mangaSubHeader.WithDivider(true);
            mangaSubHeader.WithTextColorRes(ResourceExtension.BrushTextRes);


            var othersSubHeader = new SectionDrawerItem();
            othersSubHeader.WithName("Other");
            othersSubHeader.WithDivider(true);
            othersSubHeader.WithTextColorRes(ResourceExtension.BrushTextRes);


            builder.WithDrawerItems(new List<IDrawerItem>()
            {
                animeButton,
                searchButton,
                seasonalButton,
                recomButton,
                topAnimeButton,
                calendarButton,
                mangaSubHeader,//
                mangaListButton,
                topMangaButton,
                othersSubHeader,//
                articlesButton,
                videoButton,
                feedsButton,
                forumsButton,
                messagingButton,

            });

            _drawer = builder.Build();
            UpdateLogInLabel();
            _drawer.StickyFooter.SetBackgroundColor(new Color(ResourceExtension.BrushAnimeItemInnerBackground));

        }

        public async Task UpdateProfileImg(bool dl = true)
        {
            
        }

        public void SetActiveButton(HamburgerButtons val)
        {
           //
        }

        public void UpdateApiDependentButtons()
        {
            //
        }

        public void UpdateAnimeFiltersSelectedIndex()
        {
            //
        }

        public async void UpdateLogInLabel()
        {
            IDrawerItem accountButton;
            if (Credentials.Authenticated)
            {
                var btn = new ProfileDrawerItem();
                btn.WithName("Account");
                btn.WithTextColorRes(ResourceExtension.BrushTextRes);
                btn.WithSelectedColorRes(ResourceExtension.BrushAnimeItemBackgroundRes);
                btn.WithSelectedTextColorRes(Resource.Color.AccentColour);
                btn.WithIdentifier((int)PageIndex.PageProfile);
                btn.WithIcon(Resource.Drawable.icon_account);
                accountButton = btn;
            }
            else
            {
                var btn = GetBaseSecondaryItem();
                btn.WithName("Sign in");
                btn.WithIdentifier((int)PageIndex.PageLogIn);
                btn.WithIcon(Resource.Drawable.icon_login);
                accountButton = btn;
            }

            var settingsButton = GetBaseSecondaryItem();
            settingsButton.WithName("Settings & more");
            settingsButton.WithIdentifier((int)PageIndex.PageSettings);
            settingsButton.WithIcon(Resource.Drawable.icon_settings);

            _drawer.RemoveAllStickyFooterItems();
            _drawer.AddStickyFooterItem(accountButton);
            _drawer.AddStickyFooterItem(settingsButton);

            if (Credentials.Authenticated)
            {
                var btn = accountButton as ProfileDrawerItem;

                try
                {
                    var bmp = await ImageService.Instance.LoadUrl(
                            $"https://myanimelist.cdn-dena.com/images/userimages/{Credentials.Id}.jpg")
                        .AsBitmapDrawableAsync();
                    btn.WithIcon(bmp);
                }
                catch (Exception) // no image available
                {
                    btn.WithIcon(Resource.Drawable.icon_account);
                }

                _drawer.UpdateStickyFooterItem(btn);
            }
        }

        public bool MangaSectionVisbility { get; set; }

        public void SetActiveButton(TopAnimeType topType)
        {
           //
        }

        public void UpdatePinnedProfiles()
        {
            //
        }

        public void UpdateBottomMargin()
        {
           //
        }
    }
}