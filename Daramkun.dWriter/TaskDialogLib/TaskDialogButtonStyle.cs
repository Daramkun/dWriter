﻿/***************************************************************************************************
 *
 *  Flatcode Task Dialog Library
 *  Copyright © 2014 Flatcode.net. All Rights Reserved.
 *
 *  File:
 *    TaskDialogButtonStyle.cs
 *  Purpose:
 *    Task dialog button style enumeration.
 *  Authors:
 *    Florian Schneidereit <florian.schneidereit@outlook.com>
 *
 *  This library is free software; you can redistribute it and/or modify it under the terms of
 *  the GNU Lesser General Public License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 *  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *  See the GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License along with this
 *  library; if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 *  Boston, MA  02110-1301  USA
 *
 **************************************************************************************************/

#region Using Directives

using System;

#endregion

namespace Flatcode.Presentation
{
	/// <summary>
	/// Specifies constants defining the style for custom buttons on a <see cref="TaskDialog"/>.
	/// </summary>
	public enum TaskDialogButtonStyle
	{
		/// <summary>
		/// Indicates that the task dialog has normal custom buttons.
		/// </summary>
		Normal,

		/// <summary>
		/// Indicates that the task dialog has command-link custom buttons.
		/// </summary>
		CommandLinks,

		/// <summary>
		/// Indicates that the task dialog has command-link custom buttons without icons.
		/// </summary>
		CommandLinksNoIcon
	}
}
