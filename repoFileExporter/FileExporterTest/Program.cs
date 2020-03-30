/**
*	Copyright (C) 2020 3D Repo Ltd
*
*	This program is free software: you can redistribute it and/or modify
*	it under the terms of the GNU Affero General Public License as
*	published by the Free Software Foundation, either version 3 of the
*	License, or (at your option) any later version.
*
*	This program is distributed in the hope that it will be useful,
*	but WITHOUT ANY WARRANTY; without even the implied warranty of
*	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*	GNU Affero General Public License for more details.
*
*	You should have received a copy of the GNU Affero General Public License
*	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using RepoFileExporter;
using RepoFileExporter.dataStructures;
using System.Collections.Generic;

namespace FileExporterTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BIMDataExporter exporter = new BIMDataExporter();
            var redMat = exporter.AddMaterial(new List<float> { 1f, 0f, 0f, 0f });

            Geometry geometry = new Geometry(
                new List<double> { 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0 },
                new List<uint> { 0, 1, 2, 0, 2, 3 }, null,
                redMat);

            var rootNode = exporter.AddNode("root", -1, null);

            Dictionary<string, RepoVariant> metadata = new Dictionary<string, RepoVariant>();
            metadata.Add("CustomMeta1", RepoVariant.String("value 2"));
            metadata.Add("Area", RepoVariant.Int(1));
            metadata.Add("Boolean Test", RepoVariant.Boolean(true));
            metadata.Add("Double", RepoVariant.Double(1.3242524));

            exporter.AddNode("mesh1", rootNode, null, geometry, metadata);

            exporter.AddNode("mesh2", rootNode,
                new List<float> {
                    1, 0, 0, 2,
                    0, 1, 0, 2,
                    0, 0, 1, 2,
                    0, 0, 0, 1
                },
                geometry);

            exporter.ExportToFile("C:\\Users\\Carmen\\Desktop\\manual.bim");
        }
    }
}