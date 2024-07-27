using CookBook.Helpers;
using CookBook.Services;
using CookBook.ViewModels;
using DomainModel.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CookBook.UI
{
    public partial class FoodManagerForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        FoodManagerCache _foodManagerCache;
        public FoodManagerForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _foodManagerCache = _serviceProvider.GetRequiredService<FoodManagerCache>();
            RecipesLbx.OnSelectedItemChanged += OnSelectedRecipeChanged;
        }

        private void OnSelectedRecipeChanged(ListBoxItemVM selectedItem)
        {
            Recipe selectedRecipe = (Recipe)selectedItem.Item;
            var ingredients = _foodManagerCache.GetIngredients(selectedRecipe.Id);
            List<ListBoxItemVM> dataSource = new List<ListBoxItemVM>();

            decimal totalKcal = 0;
            decimal totalPrice = 0;

            foreach (RecipeIngredientExtendedVM ingredient in ingredients)
            {
                ListBoxItemVM item = new ListBoxItemVM(ingredient, ingredient.NameWithMissingAmount);
                dataSource.Add(item);

                totalKcal += (ingredient.KcalPer100g / 100) * ingredient.Amount;
                totalPrice += (ingredient.PricePer100g / 100) * ingredient.Amount;
            }
            IngredientsLbx.SetDataSource(dataSource);

            DescriptionTxt.Text = selectedRecipe.Description;
            if (selectedRecipe.Image != null)
                RecipePicture.Image = ImageHelper.ConvertFromDbImage(selectedRecipe.Image);
            else
                RecipePicture.Image = ImageHelper.PlaceholderImage;

            TotalCaloriesLbl.Text = totalKcal.ToString();
            TotalPriceLbl.Text = totalPrice.ToString();
        }

        private async void FoodManagerForm_Load(object sender, EventArgs e)
        {
            RecipePicture.SizeMode = PictureBoxSizeMode.StretchImage;
            await _foodManagerCache.RefreshData();
        }

        private void AvailableBtn_Click(object sender, EventArgs e)
        {
            List<ListBoxItemVM> dataSource = new List<ListBoxItemVM>();
            foreach (Recipe recipe in _foodManagerCache.AvailableRecipes)
            {
                ListBoxItemVM item = new ListBoxItemVM(recipe, recipe.Name);
                dataSource.Add(item);
            }
            RecipesLbx.SetDataSource(dataSource);

            PrepareFoodBtn.Visible = true;
            CreateShoppingListBtn.Visible = false;
        }

        private void UnavailableBtn_Click(object sender, EventArgs e)
        {
            List<ListBoxItemVM> dataSource = new List<ListBoxItemVM>();
            foreach (Recipe recipe in _foodManagerCache.UnavailableRecipes)
            {
                ListBoxItemVM item = new ListBoxItemVM(recipe, recipe.Name);
                dataSource.Add(item);
            }
            RecipesLbx.SetDataSource(dataSource);

            PrepareFoodBtn.Visible = false;
            CreateShoppingListBtn.Visible = true;
        }

        private async void PrepareFoodBtn_Click(object sender, EventArgs e)
        {
            Recipe selectedRecipe = (Recipe) RecipesLbx.SelectedItem.Item;

            await _foodManagerCache.PrepareFood(selectedRecipe);
        }
    }
}
