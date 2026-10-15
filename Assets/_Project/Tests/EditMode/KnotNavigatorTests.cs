using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaveGame.Tests
{
    public class KnotNavigatorTests
    {
        private List<KnotDefinition> _knots;

        [SetUp]
        public void SetUp()
        {
            _knots = new List<KnotDefinition>();
            foreach (var name in new[] { "Восьмёрка", "Штык", "Булинь" })
            {
                var knot = ScriptableObject.CreateInstance<KnotDefinition>();
                knot.displayName = name;
                _knots.Add(knot);
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var knot in _knots) Object.DestroyImmediate(knot);
        }

        [Test]
        public void StartsAtFirstKnot()
        {
            var navigator = new KnotNavigator(_knots);
            Assert.AreEqual(0, navigator.CurrentIndex);
            Assert.AreSame(_knots[0], navigator.Current);
            Assert.AreEqual(3, navigator.Count);
        }

        [Test]
        public void MoveNext_GoesToNextKnotInCatalogOrder()
        {
            var navigator = new KnotNavigator(_knots);
            Assert.IsTrue(navigator.HasNext);
            Assert.IsTrue(navigator.MoveNext());
            Assert.AreEqual(1, navigator.CurrentIndex);
            Assert.AreEqual("Штык", navigator.Current.displayName);
        }

        [Test]
        public void LastKnot_HasNoNextAndStaysPut()
        {
            var navigator = new KnotNavigator(_knots, 2);
            Assert.IsFalse(navigator.HasNext);
            Assert.IsFalse(navigator.MoveNext());
            Assert.AreEqual(2, navigator.CurrentIndex);
        }

        [Test]
        public void SingleKnotCatalog_HasNoNext()
        {
            var navigator = new KnotNavigator(_knots.GetRange(0, 1));
            Assert.IsFalse(navigator.HasNext);
        }

        [Test]
        public void Select_JumpsToKnotByIndex()
        {
            var navigator = new KnotNavigator(_knots);
            navigator.Select(2);
            Assert.AreEqual(2, navigator.CurrentIndex);
            Assert.AreEqual("Булинь", navigator.Current.displayName);
        }

        [Test]
        public void Select_OutOfRange_Throws()
        {
            var navigator = new KnotNavigator(_knots);
            Assert.Throws<ArgumentOutOfRangeException>(() => navigator.Select(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => navigator.Select(-1));
            Assert.AreEqual(0, navigator.CurrentIndex);
        }

        [Test]
        public void EmptyCatalog_Throws()
        {
            Assert.Throws<ArgumentException>(() => new KnotNavigator(new List<KnotDefinition>()));
        }

        [Test]
        public void Catalog_SkipsMissingEntries()
        {
            // Пустой слот в списке каталога (узел ещё не создан) не попадает в навигацию.
            var catalog = ScriptableObject.CreateInstance<KnotCatalog>();
            catalog.knots = new List<KnotDefinition> { _knots[0], null, _knots[2] };
            try
            {
                var available = catalog.AvailableKnots;
                Assert.AreEqual(2, available.Count);
                Assert.AreSame(_knots[2], available[1]);
            }
            finally
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void Selection_RemembersChoiceUntilReset()
        {
            KnotSelection.Reset();
            Assert.IsNull(KnotSelection.Navigator);

            KnotSelection.Select(_knots, 1);
            Assert.AreEqual(1, KnotSelection.Navigator.CurrentIndex);
            Assert.AreSame(_knots[1], KnotSelection.Navigator.Current);

            KnotSelection.Reset();
            Assert.IsNull(KnotSelection.Navigator);
        }
    }
}
