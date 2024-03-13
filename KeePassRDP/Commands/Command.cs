/*
 *  Copyright (C) 2018 - 2024 iSnackyCracky, NETertainer
 *
 *  This file is part of KeePassRDP.
 *
 *  KeePassRDP is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  KeePassRDP is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using KeePassRDP.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace KeePassRDP.Commands
{
    internal interface ICommand
    {
        string Executable { get; }
        string ToString();
    }

    internal abstract class Command : ICommand
    {
        private static readonly ConcurrentDictionary<string, bool> _fileExistsCache = new ConcurrentDictionary<string, bool>(2, 0);
        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<Tuple<PropertyInfo, CommandArgumentAttribute>>> _propertyCache = new ConcurrentDictionary<Type, ReadOnlyCollection<Tuple<PropertyInfo, CommandArgumentAttribute>>>(2, 0);

        private readonly string _executable;
        public string Executable { get { return _executable; } }

        public Command(string executable)
        {
            if (!_fileExistsCache.AddOrUpdate(executable, x => File.Exists(x), (x, y) => y ? y : File.Exists(x)))
                throw new FileNotFoundException(executable);

            _executable = executable;
        }

        public override string ToString()
        {
            var argumentsBuilder = new StringBuilder();

            foreach (var prop in _propertyCache.GetOrAdd(GetType(), t => t
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(x =>
                {
                    var commandArgumentAttribute = x.GetCustomAttributes(false).OfType<CommandArgumentAttribute>().FirstOrDefault();
                    if (commandArgumentAttribute == null)
                        return null;
                    return new Tuple<PropertyInfo, CommandArgumentAttribute>(x, commandArgumentAttribute);
                })
                .Where(x => x != null && x.Item2 != null)
                .OrderBy(x => x.Item2.Position)
                .ToArray()
                .AsReadOnly())
            .Select(x => new { Value = x.Item1.GetValue(this, null), Attribute = x.Item2 })
            .Where(x => x.Value != null))
            {
                var value = prop.Value;
                var attribute = prop.Attribute;

                if (value != null && (!(value is bool) || (bool)value == true))
                {
                    if (!string.IsNullOrEmpty(attribute.Parameter))
                    {
                        argumentsBuilder.Append(attribute.Prefix);
                        argumentsBuilder.Append(attribute.Parameter);
                        if (attribute.Delimiter != char.MinValue)
                        {
                            var stringValue = value.ToString();
                            if (!string.IsNullOrEmpty(stringValue))
                            {
                                argumentsBuilder.Append(attribute.Delimiter);
                                argumentsBuilder.Append(stringValue);
                            }
                        }
                        argumentsBuilder.Append(' ');
                    }
                    else
                    {
                        var stringValue = value.ToString();
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            argumentsBuilder.Append('"');
                            argumentsBuilder.Append(stringValue);
                            argumentsBuilder.Append('"');
                            argumentsBuilder.Append(' ');
                        }
                    }
                }
            }

            return argumentsBuilder.Length > 0 ? argumentsBuilder.ToString() : string.Empty;
        }
    }

    internal abstract class Command<T> : Command where T : new()
    {
        public Command(string executable) : base(executable)
        {
        }

        /*public T Extend(T input)
        {
            var extended = new T();
            foreach (var prop in typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = prop.GetValue(input, null) ?? prop.GetValue(this, null);
                if (value != null && (!(value is bool) || (bool)value == true))
                    prop.SetValue(extended, value, null);
            }
            return extended;
        }*/
    }
}
