using Gpt.Labs.Models;
using Gpt.Labs.Models.Interfaces;
using Gpt.Labs.ViewModels.Collections;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Gpt.Labs.Helpers.Navigation;

namespace Gpt.Labs.ViewModels.Base
{
    public abstract class ListViewModel<TElement> : StateViewModelBase
        where TElement : class, IEntity<Guid>
    {
        #region Fields

        private ObservableList<TElement, Guid> itemsCollection;

        private TElement selectedElement;

        #endregion

        #region Properties

        public ObservableList<TElement, Guid> ItemsCollection
        {
            get => this.itemsCollection;
            protected set => this.Set(ref this.itemsCollection, value);
        }

        public TElement SelectedElement
        {
            get => this.selectedElement;
            set
            {
                var oldValue = this.selectedElement;
                if (this.Set(ref this.selectedElement, value) && typeof(ISelectable).IsAssignableFrom(typeof(TElement)))
                {
                    this.OnSelectedElementChanged();

                    if (oldValue is ISelectable oldElement)
                    {
                        oldElement.IsSelected = false;
                    }

                    if (this.selectedElement is ISelectable newElement)
                    {
                        newElement.IsSelected = true;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetSelection(Guid id)
        {
            if (this.itemsCollection == null)
            {
                return;
            }

            this.SelectedElement = this.ItemsCollection.GetById(id);
        }

        public override Task LoadStateAsync(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            Guid? selectedElementId = null;

            if (mode == NavigationMode.New)
            {
               selectedElementId = parameters.GetValue<Guid?>("selection-id");
            }
            else
            {
                selectedElementId = state.GetValue<Guid?>("SelectedElementId");
            }

            if (selectedElementId.HasValue)
            {
                this.SetSelection(selectedElementId.Value);
            }

            return Task.CompletedTask;
        }

        public override void SaveState(Type destinationPageType, Query parameters, ViewModelState state, NavigationMode mode)
        {
            if (this.SelectedElement != null)
            {
                state.SetValue("SelectedElementId", this.SelectedElement.Id);
            }
        }

        public virtual void OnSelectedElementChanged()
        {

        }

        #endregion
    }
}
