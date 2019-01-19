﻿using System;
using M11.Common.Extentions;
using Xamarin.Forms.Xaml;

namespace M11
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : BaseContentPage
    {
		public SettingsPage ()
		{
			InitializeComponent();
		    NotificationFrequencyPicker.ItemsSource = App.NotificationFrequenciesDescriptions;
		    NotificationFrequencyPicker.SelectedItem = App.NotificationFrequency.GetDescription();
		    NotificationFrequencyDescriptionLabel.Text = App.NotificationFrequency.GetFullDescription();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            App.Exit();
        }

        private void NotificationFrequencyPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            App.NotificationFrequency = App.NotificationFrequencies[NotificationFrequencyPicker.SelectedIndex];
        }
    }
}