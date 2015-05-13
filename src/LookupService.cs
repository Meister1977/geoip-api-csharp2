/**
 * LookupService.cs
 *
 * Copyright (C) 2008 MaxMind Inc.  All Rights Reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */


using System;
using System.IO;
using System.Net;

namespace LMI.GeoIp
{
    internal class LookupService
    {
        private const int COUNTRY_BEGIN = 16776960;
        private const int STRUCTURE_INFO_MAX_SIZE = 20;
        private const int FULL_RECORD_LENGTH = 100; //???
        private const int SEGMENT_RECORD_LENGTH = 3;
        private const int STANDARD_RECORD_LENGTH = 3;
        private const int ORG_RECORD_LENGTH = 4;
        private const int MAX_RECORD_LENGTH = 4;
        private const int STATE_BEGIN_REV0 = 16700000;
        private const int STATE_BEGIN_REV1 = 16000000;
        private static readonly Country UNKNOWN_COUNTRY = new Country("XX", "N/A");
        public const int GEOIP_STANDARD = 0;
        public const int GEOIP_MEMORY_CACHE = 1;
        public const int GEOIP_UNKNOWN_SPEED = 0;
        public const int GEOIP_DIALUP_SPEED = 1;
        public const int GEOIP_CABLEDSL_SPEED = 2;
        public const int GEOIP_CORPORATE_SPEED = 3;

        private static readonly String[] CountryCode =
        {
            "XX", "AP", "EU", "AD", "AE", "AF", "AG", "AI", "AL", "AM", "CW",
            "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AZ", "BA", "BB",
            "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BM", "BN", "BO",
            "BR", "BS", "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD",
            "CF", "CG", "CH", "CI", "CK", "CL", "CM", "CN", "CO", "CR",
            "CU", "CV", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO",
            "DZ", "EC", "EE", "EG", "EH", "ER", "ES", "ET", "FI", "FJ",
            "FK", "FM", "FO", "FR", "SX", "GA", "GB", "GD", "GE", "GF",
            "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT",
            "GU", "GW", "GY", "HK", "HM", "HN", "HR", "HT", "HU", "ID",
            "IE", "IL", "IN", "IO", "IQ", "IR", "IS", "IT", "JM", "JO",
            "JP", "KE", "KG", "KH", "KI", "KM", "KN", "KP", "KR", "KW",
            "KY", "KZ", "LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT",
            "LU", "LV", "LY", "MA", "MC", "MD", "MG", "MH", "MK", "ML",
            "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV",
            "MW", "MX", "MY", "MZ", "NA", "NC", "NE", "NF", "NG", "NI",
            "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF",
            "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW",
            "PY", "QA", "RE", "RO", "RU", "RW", "SA", "SB", "SC", "SD",
            "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO",
            "SR", "ST", "SV", "SY", "SZ", "TC", "TD", "TF", "TG", "TH",
            "TJ", "TK", "TM", "TN", "TO", "TL", "TR", "TT", "TV", "TW",
            "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC", "VE",
            "VG", "VI", "VN", "VU", "WF", "WS", "YE", "YT", "RS", "ZA",
            "ZM", "ME", "ZW", "A1", "A2", "O1", "AX", "GG", "IM", "JE",
            "BL", "MF", "BQ", "SS", "O1"
        };

        private static readonly String[] CountryName =
        {
            "N/A", "Asia/Pacific Region", "Europe", "Andorra", "United Arab Emirates", "Afghanistan", "Antigua and Barbuda",
            "Anguilla", "Albania", "Armenia", "Curacao",
            "Angola", "Antarctica", "Argentina", "American Samoa", "Austria", "Australia", "Aruba", "Azerbaijan",
            "Bosnia and Herzegovina", "Barbados",
            "Bangladesh", "Belgium", "Burkina Faso", "Bulgaria", "Bahrain", "Burundi", "Benin", "Bermuda",
            "Brunei Darussalam", "Bolivia",
            "Brazil", "Bahamas", "Bhutan", "Bouvet Island", "Botswana", "Belarus", "Belize", "Canada",
            "Cocos (Keeling) Islands", "Congo, The Democratic Republic of the",
            "Central African Republic", "Congo", "Switzerland", "Cote D'Ivoire", "Cook Islands", "Chile", "Cameroon",
            "China", "Colombia", "Costa Rica",
            "Cuba", "Cape Verde", "Christmas Island", "Cyprus", "Czech Republic", "Germany", "Djibouti", "Denmark",
            "Dominica", "Dominican Republic",
            "Algeria", "Ecuador", "Estonia", "Egypt", "Western Sahara", "Eritrea", "Spain", "Ethiopia", "Finland", "Fiji",
            "Falkland Islands (Malvinas)", "Micronesia, Federated States of", "Faroe Islands", "France",
            "Sint Maarten (Dutch part)", "Gabon", "United Kingdom", "Grenada", "Georgia", "French Guiana",
            "Ghana", "Gibraltar", "Greenland", "Gambia", "Guinea", "Guadeloupe", "Equatorial Guinea", "Greece",
            "South Georgia and the South Sandwich Islands", "Guatemala",
            "Guam", "Guinea-Bissau", "Guyana", "Hong Kong", "Heard Island and McDonald Islands", "Honduras", "Croatia",
            "Haiti", "Hungary", "Indonesia",
            "Ireland", "Israel", "India", "British Indian Ocean Territory", "Iraq", "Iran, Islamic Republic of", "Iceland",
            "Italy", "Jamaica", "Jordan",
            "Japan", "Kenya", "Kyrgyzstan", "Cambodia", "Kiribati", "Comoros", "Saint Kitts and Nevis",
            "Korea, Democratic People's Republic of", "Korea, Republic of", "Kuwait",
            "Cayman Islands", "Kazakhstan", "Lao People's Democratic Republic", "Lebanon", "Saint Lucia", "Liechtenstein",
            "Sri Lanka", "Liberia", "Lesotho", "Lithuania",
            "Luxembourg", "Latvia", "Libya", "Morocco", "Monaco", "Moldova, Republic of", "Madagascar", "Marshall Islands",
            "Macedonia", "Mali",
            "Myanmar", "Mongolia", "Macau", "Northern Mariana Islands", "Martinique", "Mauritania", "Montserrat", "Malta",
            "Mauritius", "Maldives",
            "Malawi", "Mexico", "Malaysia", "Mozambique", "Namibia", "New Caledonia", "Niger", "Norfolk Island", "Nigeria",
            "Nicaragua",
            "Netherlands", "Norway", "Nepal", "Nauru", "Niue", "New Zealand", "Oman", "Panama", "Peru", "French Polynesia",
            "Papua New Guinea", "Philippines", "Pakistan", "Poland", "Saint Pierre and Miquelon", "Pitcairn Islands",
            "Puerto Rico", "Palestinian Territory", "Portugal", "Palau",
            "Paraguay", "Qatar", "Reunion", "Romania", "Russian Federation", "Rwanda", "Saudi Arabia", "Solomon Islands",
            "Seychelles", "Sudan",
            "Sweden", "Singapore", "Saint Helena", "Slovenia", "Svalbard and Jan Mayen", "Slovakia", "Sierra Leone",
            "San Marino", "Senegal", "Somalia", "Suriname",
            "Sao Tome and Principe", "El Salvador", "Syrian Arab Republic", "Swaziland", "Turks and Caicos Islands", "Chad",
            "French Southern Territories", "Togo", "Thailand",
            "Tajikistan", "Tokelau", "Turkmenistan", "Tunisia", "Tonga", "Timor-Leste", "Turkey", "Trinidad and Tobago",
            "Tuvalu", "Taiwan",
            "Tanzania, United Republic of", "Ukraine", "Uganda", "United States Minor Outlying Islands", "United States",
            "Uruguay", "Uzbekistan", "Holy See (Vatican City State)", "Saint Vincent and the Grenadines", "Venezuela",
            "Virgin Islands, British", "Virgin Islands, U.S.", "Vietnam", "Vanuatu", "Wallis and Futuna", "Samoa", "Yemen",
            "Mayotte", "Serbia", "South Africa",
            "Zambia", "Montenegro", "Zimbabwe", "Anonymous Proxy", "Satellite Provider", "Other", "Aland Islands",
            "Guernsey", "Isle of Man", "Jersey",
            "Saint Barthelemy", "Saint Martin", "Bonaire, Saint Eustatius and Saba", "South Sudan", "Other"
        };

        private readonly int[] _databaseSegments;
        private readonly byte _databaseType = Convert.ToByte(DatabaseInfo.COUNTRY_EDITION);
        private readonly byte[] _dbbuffer;
        private readonly int _recordLength;

        public LookupService(String databaseFile)
        {
            using (var file = new FileStream(databaseFile, FileMode.Open, FileAccess.Read, FileShare.Read, 8192))
            {
                var l = (int)file.Length;
                _dbbuffer = new byte[l];
                file.Seek(0, SeekOrigin.Begin);
                file.Read(_dbbuffer, 0, l);
            }


            var delim = new byte[3];
            var buf = new byte[SEGMENT_RECORD_LENGTH];
            _databaseType = DatabaseInfo.COUNTRY_EDITION;
            _recordLength = STANDARD_RECORD_LENGTH;

            var pos = _dbbuffer.Length - 3;
            for (var i = 0; i < STRUCTURE_INFO_MAX_SIZE; i++)
            {
                Array.Copy(_dbbuffer, pos, delim, 0, 3);
                pos += 3;
                if (delim[0] != 255 || delim[1] != 255 || delim[2] != 255)
                {
                    pos -= 4;
                }
                else
                {
                    _databaseType = _dbbuffer[pos];
                    pos++;

                    if (_databaseType >= 106)
                    {
                        // Backward compatibility with databases from April 2003 and earlier
                        _databaseType -= 105;
                    }
                    // Determine the database type.
                    if (_databaseType == DatabaseInfo.REGION_EDITION_REV0)
                    {
                        _databaseSegments = new int[1];
                        _databaseSegments[0] = STATE_BEGIN_REV0;
                        _recordLength = STANDARD_RECORD_LENGTH;
                    }
                    else if (_databaseType == DatabaseInfo.REGION_EDITION_REV1)
                    {
                        _databaseSegments = new int[1];
                        _databaseSegments[0] = STATE_BEGIN_REV1;
                        _recordLength = STANDARD_RECORD_LENGTH;
                    }
                    else if (_databaseType == DatabaseInfo.CITY_EDITION_REV0 ||
                             _databaseType == DatabaseInfo.CITY_EDITION_REV1 ||
                             _databaseType == DatabaseInfo.ORG_EDITION ||
                             _databaseType == DatabaseInfo.ORG_EDITION_V6 ||
                             _databaseType == DatabaseInfo.ISP_EDITION ||
                             _databaseType == DatabaseInfo.ISP_EDITION_V6 ||
                             _databaseType == DatabaseInfo.ASNUM_EDITION ||
                             _databaseType == DatabaseInfo.ASNUM_EDITION_V6 ||
                             _databaseType == DatabaseInfo.NETSPEED_EDITION_REV1 ||
                             _databaseType == DatabaseInfo.NETSPEED_EDITION_REV1_V6 ||
                             _databaseType == DatabaseInfo.CITY_EDITION_REV0_V6 ||
                             _databaseType == DatabaseInfo.CITY_EDITION_REV1_V6
                        )
                    {
                        _databaseSegments = new int[1];
                        _databaseSegments[0] = 0;
                        if (_databaseType == DatabaseInfo.CITY_EDITION_REV0 ||
                            _databaseType == DatabaseInfo.CITY_EDITION_REV1 ||
                            _databaseType == DatabaseInfo.ASNUM_EDITION_V6 ||
                            _databaseType == DatabaseInfo.NETSPEED_EDITION_REV1 ||
                            _databaseType == DatabaseInfo.NETSPEED_EDITION_REV1_V6 ||
                            _databaseType == DatabaseInfo.CITY_EDITION_REV0_V6 ||
                            _databaseType == DatabaseInfo.CITY_EDITION_REV1_V6 ||
                            _databaseType == DatabaseInfo.ASNUM_EDITION
                            )
                        {
                            _recordLength = STANDARD_RECORD_LENGTH;
                        }
                        else
                        {
                            _recordLength = ORG_RECORD_LENGTH;
                        }

                        Array.Copy(_dbbuffer, pos, buf, 0, SEGMENT_RECORD_LENGTH);

                        for (var j = 0; j < SEGMENT_RECORD_LENGTH; j++)
                        {
                            _databaseSegments[0] += (unsignedByteToInt(buf[j]) << (j * 8));
                        }
                    }
                    break;
                }
            }
            if ((_databaseType == DatabaseInfo.COUNTRY_EDITION) ||
                (_databaseType == DatabaseInfo.COUNTRY_EDITION_V6) ||
                (_databaseType == DatabaseInfo.PROXY_EDITION) ||
                (_databaseType == DatabaseInfo.NETSPEED_EDITION))
            {
                _databaseSegments = new int[1];
                _databaseSegments[0] = COUNTRY_BEGIN;
                _recordLength = STANDARD_RECORD_LENGTH;
            }
        }
   
        public Country getCountry(IPAddress ipAddress)
        {
            return getCountry(bytestoLong(ipAddress.GetAddressBytes()));
        }

        public Country getCountry(String ipAddress)
        {
            IPAddress addr;
            try
            {
                addr = IPAddress.Parse(ipAddress);
            }
            catch (FormatException) 
            {
                return UNKNOWN_COUNTRY;
            }

            return getCountry(bytestoLong(addr.GetAddressBytes()));
        }


        public Country getCountry(long ipAddress)
        {

            if ((_databaseType == DatabaseInfo.CITY_EDITION_REV1) |
                (_databaseType == DatabaseInfo.CITY_EDITION_REV0))
            {
                Country l = getLocation(ipAddress);
                if (l == null)
                {
                    return UNKNOWN_COUNTRY;
                }
                return l;
            }
            int ret = SeekCountry(ipAddress) - COUNTRY_BEGIN;
            if (ret == 0)
            {
                return UNKNOWN_COUNTRY;
            }
            return new Country(CountryCode[ret], CountryName[ret]);
        }

        public Country getLocation(long ipnum)
        {
            var record_buf = new byte[FULL_RECORD_LENGTH];
            var record_buf2 = new char[FULL_RECORD_LENGTH];

            int Seek_country = SeekCountry(ipnum);
            if (Seek_country == _databaseSegments[0])
            {
                return null;
            }
            int record_pointer = Seek_country + ((2*_recordLength - 1)*_databaseSegments[0]);

            Array.Copy(_dbbuffer, record_pointer, record_buf, 0,
                Math.Min(_dbbuffer.Length - record_pointer, FULL_RECORD_LENGTH));
            for (int a0 = 0; a0 < FULL_RECORD_LENGTH; a0++)
            {
                record_buf2[a0] = Convert.ToChar(record_buf[a0]);
            }
            return new Country(CountryCode[unsignedByteToInt(record_buf[0])],
                CountryName[unsignedByteToInt(record_buf[0])]);

        }


        private int SeekCountry(long ipAddress)
        {
            var buf = new byte[2*MAX_RECORD_LENGTH];
            var x = new int[2];
            int offset = 0;
            for (int depth = 31; depth >= 0; depth--)
            {
                for (int i = 0; i < (2*MAX_RECORD_LENGTH); i++)
                {
                    buf[i] = _dbbuffer[i + (2*_recordLength*offset)];
                }

                for (int i = 0; i < 2; i++)
                {
                    x[i] = 0;
                    for (int j = 0; j < _recordLength; j++)
                    {
                        int y = buf[(i*_recordLength) + j];
                        if (y < 0)
                        {
                            y += 256;
                        }
                        x[i] += (y << (j*8));
                    }
                }

                if ((ipAddress & (1 << depth)) > 0)
                {
                    if (x[1] >= _databaseSegments[0])
                    {
                        return x[1];
                    }
                    offset = x[1];
                }
                else
                {
                    if (x[0] >= _databaseSegments[0])
                    {
                        return x[0];
                    }
                    offset = x[0];
                }
            }

            // shouldn't reach here
            throw new Exception("Error Seeking country while Seeking " + ipAddress);
        }

        private static long bytestoLong(byte[] address)
        {
            long ipnum = 0;
            for (int i = 0; i < 4; ++i)
            {
                long y = address[i];
                if (y < 0)
                {
                    y += 256;
                }
                ipnum += y << ((3 - i)*8);
            }
            return ipnum;
        }

        private static int unsignedByteToInt(byte b)
        {
            return b & 0xFF;
        }
    }
}
