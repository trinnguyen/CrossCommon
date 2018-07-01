using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrossCommon.Mvvm
{
    public class BindableBase : INotifyPropertyChanged
    {
        public BindableBase()
        {
        }

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Set new value to propery to
        /// </summary>
        /// <returns><c>true</c>, if property was set, <c>false</c> otherwise.</returns>
        /// <param name="curVal">Current value.</param>
        /// <param name="newVal">New value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected bool SetProperty<T>(ref T curVal, T newVal, [CallerMemberName] string propertyName = null)
        {
            // check if already equal, do nothing
            if (EqualityComparer<T>.Default.Equals(curVal, newVal))
                return false;

            // update to new value
            curVal = newVal;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, args);
        }
    }
}
