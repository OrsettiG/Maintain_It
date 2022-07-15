using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.ViewModels;
using Maintain_it.Helpers;

namespace MaintainIt.Tests.ViewModels
{
    [TestFixture]
    internal class MaintenanceItemViewModelTests
    {
        #region Initialization
        [Test]
        public void Constructor_Created_RecursEveryIsZero()
        {
            // Arrange
            MaintenanceItemViewModel vm = new();
            // Act
            // Assert
            Assert.That( vm.RecursEvery, Is.EqualTo( 0 ) );
        }
        #endregion

        #region OnIncrementCommand
        [Test]
        public void OnIncrementCommand_Executed_IncrementsRecursEveryByOne()
        {
            // Arrange
            MaintenanceItemViewModel vm = new();

            // Act
            vm.OnIncrementCommand.Execute( null );

            // Assert
            Assert.That( vm.RecursEvery, Is.EqualTo( 1 ) );
        }
        #endregion

        #region OnDecrementCommand
        [Test]
        public void OnDecrementCommand_Executed_DoesNotDecrementRecursEveryBelowZero()
        {
            // Arrange
            MaintenanceItemViewModel vm = new();

            // Act
            vm.OnDecrementCommand.Execute( null );

            // Assert
            Assert.That( vm.RecursEvery, Is.EqualTo( 0 ) );
        }

        [Test]
        public void OnDecrementCommand_Executed_DecrementsRecursEveryByOne()
        {
            // Arrange
            MaintenanceItemViewModel vm = new()
            {
                RecursEvery = 2
            };

            // Act
            vm.OnDecrementCommand.Execute( null );

            // Assert
            Assert.That( vm.RecursEvery, Is.EqualTo( 1 ) );
        }
        #endregion


    }
}
