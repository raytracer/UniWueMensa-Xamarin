using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;

namespace uniwuemensa
{
	public class SettingsPage : ContentPage
	{
		public SettingsPage()
		{
            Title = "Einstellungen";

            var currentSettings = Settings.Instance.settings;

            var priceDict = new Dictionary<string, JsonPrice> 
			{
                { "Student", JsonPrice.Student },
                { "Mitarbeiter", JsonPrice.Staff },
                { "Gast", JsonPrice.Guest }
            };

            var pricePickerLabel = new Label
            {
                Text = "PreisTyp",
				FontSize = 18.0
            };
            var pricePicker = new Picker();

            foreach (var priceTypeName in priceDict.Keys)
            {
				var preSelectedPrice = currentSettings.Price;
				pricePicker.Items.Add(priceTypeName);
                if (priceDict[priceTypeName] == preSelectedPrice)
                {
					pricePicker.SelectedItem = priceTypeName;
                }
            }

            pricePicker.SelectedIndexChanged += (sender, args) =>
            {
                if (pricePicker.SelectedIndex != -1)
                {
                    Settings.Instance.settings = new JsonSettings
                    {
						Price = priceDict[pricePicker.Items[pricePicker.SelectedIndex]],
                        Cafeterias = currentSettings.Cafeterias
                    };
                }
            };

            var cafeteriasLabel = new Label
            {
                Text = "Mensen",
				FontSize = 18.0
            };

            var stack = new StackLayout
            {
                Children = {
					pricePickerLabel,
					pricePicker,
                    cafeteriasLabel
                },
				Padding = 15.0
            };

            var switches = new Dictionary<string, Switch>();

            foreach (var cafeteria in Settings.AllCafeterias)
            {
                var cafeteriaSwitch = new Switch
                {
                    IsToggled = currentSettings.Cafeterias.Contains(cafeteria)
                };

                switches.Add(cafeteria, cafeteriaSwitch);

                cafeteriaSwitch.Toggled += (sender, args) =>
                {
                    Settings.Instance.settings = new JsonSettings
                    {
						Price = currentSettings.Price,
                        Cafeterias = Settings.AllCafeterias.Where(c => switches[c].IsToggled).ToArray()
                    };
                };

                var cafeteriaLabel = new Label
                {
                    Text = cafeteria,
                    FontSize = 16.0,
                    HorizontalOptions = LayoutOptions.StartAndExpand
                };

                var row = new StackLayout
                {
                    Children = {
                        cafeteriaLabel,
                        cafeteriaSwitch
                    },
                    Orientation = StackOrientation.Horizontal,
                    VerticalOptions = LayoutOptions.Center
                };

                stack.Children.Add(row);
            }


			Content = stack;
        }
	}
}
