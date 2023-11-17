/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Linq;
using Python.Runtime;
using System.Collections.Generic;

namespace QuantConnect.Data.UniverseSelection
{
    /// <summary>
    /// Defines a universe that reads fundamental us equity data
    /// </summary>
    public class FundamentalUniverse : Universe
    {
        private readonly UniverseSettings _universeSettings;
        private readonly Func<IEnumerable<Fundamental.Fundamental>, IEnumerable<Symbol>> _selector;

        /// <summary>
        /// Gets the settings used for subscriptons added for this universe
        /// </summary>
        public override UniverseSettings UniverseSettings => _universeSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundamentalUniverse"/> class
        /// </summary>
        /// <param name="universeSettings">The settings used for new subscriptions generated by this universe</param>
        /// <param name="selector">Returns the symbols that should be included in the universe</param>
        public FundamentalUniverse(UniverseSettings universeSettings, Func<IEnumerable<Fundamental.Fundamental>, IEnumerable<Symbol>> selector)
            : base(CreateConfiguration(Fundamental.Fundamentals.CreateUniverseSymbol(QuantConnect.Market.USA)))
        {
            _universeSettings = universeSettings;
            _selector = selector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundamentalUniverse"/> class
        /// </summary>
        /// <param name="universeSettings">The settings used for new subscriptions generated by this universe</param>
        /// <param name="selector">Returns the symbols that should be included in the universe</param>
        public FundamentalUniverse(UniverseSettings universeSettings, PyObject selector)
            : this(Fundamental.Fundamentals.CreateUniverseSymbol(QuantConnect.Market.USA), universeSettings, selector)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundamentalUniverse"/> class
        /// </summary>
        /// <param name="symbol">Defines the symbol to use for this universe</param>
        /// <param name="universeSettings">The settings used for new subscriptions generated by this universe</param>
        /// <param name="selector">Returns the symbols that should be included in the universe</param>
        public FundamentalUniverse(Symbol symbol, UniverseSettings universeSettings, Func<IEnumerable<Fundamental.Fundamental>, IEnumerable<Symbol>> selector)
            : base(CreateConfiguration(symbol))
        {
            _universeSettings = universeSettings;
            _selector = selector;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundamentalUniverse"/> class
        /// </summary>
        /// <param name="symbol">Defines the symbol to use for this universe</param>
        /// <param name="universeSettings">The settings used for new subscriptions generated by this universe</param>
        /// <param name="selector">Returns the symbols that should be included in the universe</param>
        public FundamentalUniverse(Symbol symbol, UniverseSettings universeSettings, PyObject selector)
            : base(CreateConfiguration(symbol))
        {
            _universeSettings = universeSettings;
            Func<IEnumerable<Fundamental.Fundamental>, object> func;
            if (selector.TryConvertToDelegate(out func))
            {
                _selector = func.ConvertToUniverseSelectionSymbolDelegate();
            }
        }

        /// <summary>
        /// Performs universe selection using the data specified
        /// </summary>
        /// <param name="utcTime">The current utc time</param>
        /// <param name="data">The symbols to remain in the universe</param>
        /// <returns>The data that passes the filter</returns>
        public override IEnumerable<Symbol> SelectSymbols(DateTime utcTime, BaseDataCollection data)
        {
            return _selector(data.Data.OfType<Fundamental.Fundamental>());
        }

        /// <summary>
        /// Creates a <see cref="Fundamental.Fundamental"/> subscription configuration for the US-equity market
        /// </summary>
        /// <param name="symbol">The symbol used in the returned configuration</param>
        /// <returns>A fundamental subscription configuration with the specified symbol</returns>
        public static SubscriptionDataConfig CreateConfiguration(Symbol symbol)
        {
            return new SubscriptionDataConfig(typeof(Fundamental.Fundamentals),
                symbol: symbol,
                resolution: Resolution.Daily,
                dataTimeZone: TimeZones.NewYork,
                exchangeTimeZone: TimeZones.NewYork,
                fillForward: false,
                extendedHours: false,
                isInternalFeed: true,
                isCustom: false,
                tickType: null,
                isFilteredSubscription: false
                );
        }
    }
}