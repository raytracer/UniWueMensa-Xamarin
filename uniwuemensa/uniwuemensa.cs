using System;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional;

namespace uniwuemensa
{
    public class Price
    {
        public string Student {get; set;}
        public string Staff {get; set;}
        public string Guest {get; set;}
    }
    public class Meal
    {
        public Price Price {get; set;}
        public string Title {get; set;}
    }

    public class Day
    {
        public Cafeteria[] Cafeterias {get; set;}
        public string DayName {get; set;}
    }

    public class Cafeteria
    {
        public string Name {get; set;}

        public Meal[] Meals {get; set;}
    }

    public class ProjectedMeal
    { 

        public string Price {get; set;}
        public string Title {get; set;}
    }

    public class MealList : List<ProjectedMeal> {
        public string Heading { get; set; }
    }

    public class App : Application
    {
        CarouselPage cPage = new CarouselPage {Title = "UniWueMensa"};
        Option<ContentPage> startPage = Option.None<ContentPage>();

        List<Day> days = new List<Day>();
        public App()
        {
            this.CreateCarouselViaProxy();

            var navigation = new NavigationPage(this.cPage);
            navigation.ToolbarItems.Add(new ToolbarItem
            {
                Text = "Optionen",
                Command = new Command(async sender => await navigation.PushAsync(new SettingsPage()))
            });
            
            MainPage = navigation;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private void UpdateView(JsonSettings settings) {
            this.cPage.Children.Clear();

            foreach (var day in days)
            {
                var stack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };

                var label = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Text = day.DayName,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 20.0,
                    TextColor = Color.White
                };

                stack.Children.Add(new ContentView
                {
                    Content = label,
                    BackgroundColor = Color.FromHex("#33B5E5"),
                    Padding = 10.0,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                });

                var grouped = day.Cafeterias
                .Where(cafeteria => settings.Cafeterias.Contains(cafeteria.Name))
                .Select(cafeteria =>
                {
                    var convertedMeals = cafeteria.Meals.Select(meal =>
                    {
                        var price = meal.Price.Student;
                        switch (settings.Price)
                        {
                            case JsonPrice.Student:
                                price = meal.Price.Student;
                                break;
                            case JsonPrice.Staff:
                                price = meal.Price.Staff;
                                break;
                            case JsonPrice.Guest:
                                price = meal.Price.Guest;
                                break;
                        }
                        return new ProjectedMeal
                        {
                            Title = meal.Title,
                            Price = price
                        };
                    });

                    var mealList = new MealList { Heading = cafeteria.Name };
                    mealList.AddRange(convertedMeals);
                    return mealList;
                });


                var listView = new ListView
                {
                    ItemsSource = grouped,
                    SeparatorVisibility = SeparatorVisibility.None,
                    HasUnevenRows = true,
                    IsPullToRefreshEnabled = true,
                    IsGroupingEnabled = true
                };

                listView.ItemTemplate = new DataTemplate(typeof(MealCell));
                listView.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));

                listView.Refreshing += (obj, args) =>
                {
                    this.CreateCarouselViaProxy();
                    listView.EndRefresh();
                };

                stack.Children.Add(listView);

                var contentPage = new ContentPage { Content = stack };

                if (day.DayName.Contains(GetDateString())) {
                    startPage = Option.Some(contentPage);
                }

                this.cPage.Children.Add(contentPage);
            }

            startPage.MatchSome(p => this.cPage.CurrentPage = p);
        }


        private async void CreateCarouselViaProxy()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://devops.raytracer.me:1234/");

            var rawDays = JArray.Parse(await response.Content.ReadAsStringAsync());
            this.days = rawDays.Select(day => day.ToObject<Day>()).ToList();

            UpdateView(Settings.Instance.settings);
            Settings.Instance.settingsObs.Subscribe(settings => UpdateView(settings));
        }

        private static string GetDateString()
        {
            var today = DateTime.Now;
            var format = "dd.MM.";

            switch (today.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    return today.AddDays(2).ToString(format);
                case DayOfWeek.Sunday:
                    return today.AddDays(1).ToString(format);
                default:
                    return today.ToString(format);
            }
        }
    }

    public class MealCell : ViewCell
    {
       public MealCell()
        {
            var horizontalLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
            var cellWrapper = new StackLayout();
            var title = new Label { TextColor = Color.FromHex("#333") };
            var price = new Label { HorizontalTextAlignment = TextAlignment.End, LineBreakMode = LineBreakMode.NoWrap, TextColor = Color.FromHex("#333") };
            title.SetBinding(Label.TextProperty, "Title");
            price.SetBinding(Label.TextProperty, "Price");

            var grid = new Grid { Padding = new Thickness(5.0) };


            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60.0, GridUnitType.Absolute) });

            grid.Children.Add(title, 0, 0);
            grid.Children.Add(price, 1, 0);

            cellWrapper.Children.Add(grid);

            base.View = cellWrapper;
        }
    }

    public class HeaderCell : ViewCell
    {
       public HeaderCell()
        {
            var title = new Label
            {
                FontSize = 18.0,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            };
            title.SetBinding(Label.TextProperty, "Heading");

            var horizontalLayout = new StackLayout
            {
                Children = {
                    title
                },
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            base.View = horizontalLayout;
        }
    }
}