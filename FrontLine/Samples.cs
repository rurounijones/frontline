// Custodian is a DCS server administration tool for Discord
// Copyright (C) 2022 Jeffrey Jones
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using RurouniJones.Dcs.FrontLine;

namespace FrontLine.Visualizer
{
    public static class Samples
    {
        public static Dictionary<string, List<UnitSite>> Sites { get; } = new Dictionary<string, List<UnitSite>>
        {
            {
                "2 Equal Points", new List<UnitSite>() {
                    new UnitSite(5, 7, CoalitionId.RedFor),
                    new UnitSite(5, 3, CoalitionId.BlueFor),
                }
            },
            {
                "North/South divide", new List<UnitSite>() {
                    new UnitSite(2, 7, CoalitionId.RedFor),
                    new UnitSite(5, 7, CoalitionId.RedFor),
                    new UnitSite(8, 7, CoalitionId.RedFor),
                    new UnitSite(2, 3, CoalitionId.BlueFor),
                    new UnitSite(5, 3, CoalitionId.BlueFor),
                    new UnitSite(8, 3, CoalitionId.BlueFor),
                }
            },
            {
                "N/S weak salient", new List<UnitSite>() {
                    new UnitSite(2, 7, CoalitionId.RedFor),
                    new UnitSite(5, 7, CoalitionId.RedFor),
                    new UnitSite(8, 8, CoalitionId.RedFor),
                    new UnitSite(2, 3, CoalitionId.BlueFor),
                    new UnitSite(5, 3, CoalitionId.BlueFor),
                    new UnitSite(8, 5, CoalitionId.BlueFor),
                }
            },
            {
                "N/S strong salient", new List<UnitSite>() {
                    new UnitSite(2, 7, CoalitionId.RedFor),
                    new UnitSite(5, 7, CoalitionId.RedFor),
                    new UnitSite(8, 9, CoalitionId.RedFor),
                    new UnitSite(2, 3, CoalitionId.BlueFor),
                    new UnitSite(5, 3, CoalitionId.BlueFor),
                    new UnitSite(8, 7, CoalitionId.BlueFor),
                }
            },
            {
                "Red 1,1 Blue 2,2", new List<UnitSite>() {
                    new UnitSite(1, 1, CoalitionId.RedFor),
                    new UnitSite(2, 2, CoalitionId.BlueFor),
                }
            },
            {
                "Red 1,1 Simple Surrounded", new List<UnitSite>() {
                    new UnitSite(1, 1, CoalitionId.RedFor),
                    new UnitSite(2, 2, CoalitionId.BlueFor),
                    new UnitSite(1, 2, CoalitionId.BlueFor),
                    new UnitSite(2, 1, CoalitionId.BlueFor)
                }
            },
            {
                "Red 1,1 Complex Surrounded", new List<UnitSite>() {
                    new UnitSite(1, 1, CoalitionId.RedFor),
                    new UnitSite(2, 2, CoalitionId.BlueFor),
                    new UnitSite(3, 3, CoalitionId.BlueFor),
                    new UnitSite(3, 4, CoalitionId.BlueFor),
                    new UnitSite(4, 3, CoalitionId.BlueFor),
                    new UnitSite(1, 2, CoalitionId.BlueFor),
                    new UnitSite(2, 1, CoalitionId.BlueFor)
                }
            },
            {
                "Red 5,5 Surrounded 4 point", new List<UnitSite>() {
                    new UnitSite(5, 5, CoalitionId.RedFor),
                    new UnitSite(2, 5, CoalitionId.BlueFor),
                    new UnitSite(5, 2, CoalitionId.BlueFor),
                    new UnitSite(8, 5, CoalitionId.BlueFor),
                    new UnitSite(5, 8, CoalitionId.BlueFor)
                }
            },
            {
                "Red 5,5 Surrounded 8 point", new List<UnitSite>() {
                    new UnitSite(5, 5, CoalitionId.RedFor),
                    new UnitSite(2, 5, CoalitionId.BlueFor),
                    new UnitSite(5, 2, CoalitionId.BlueFor),
                    new UnitSite(8, 5, CoalitionId.BlueFor),
                    new UnitSite(5, 8, CoalitionId.BlueFor),
                    new UnitSite(3, 3, CoalitionId.BlueFor),
                    new UnitSite(3, 7, CoalitionId.BlueFor),
                    new UnitSite(7, 3, CoalitionId.BlueFor),
                    new UnitSite(7, 7, CoalitionId.BlueFor),
                }
            },
        };
    }
}
