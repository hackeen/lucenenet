﻿using Lucene.Net.Index;
using Lucene.Net.Queries.Function;
using Lucene.Net.Search;
using Lucene.Net.Util;
using System;

namespace Lucene.Net.Spatial.Util
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// Filter that matches all documents where a valuesource is
    /// in between a range of <c>min</c> and <c>max</c> inclusive.
    /// @lucene.internal
    /// </summary>
    public class ValueSourceFilter : Filter
    {
        //TODO see https://issues.apache.org/jira/browse/LUCENE-4251  (move out of spatial & improve)

        readonly Filter startingFilter;
        readonly ValueSource source;
        public readonly double min;
        public readonly double max;

        public ValueSourceFilter(Filter startingFilter, ValueSource source, double min, double max)
        {
            if (startingFilter == null)
            {
                throw new ArgumentException("please provide a non-null startingFilter; you can use QueryWrapperFilter(MatchAllDocsQuery) as a no-op filter", "startingFilter");
            }
            this.startingFilter = startingFilter;
            this.source = source;
            this.min = min;
            this.max = max;
        }

        public override DocIdSet GetDocIdSet(AtomicReaderContext context, Bits acceptDocs)
        {
            var values = source.GetValues(null, context);
            return new ValueSourceFilteredDocIdSet(this, startingFilter.GetDocIdSet(context, acceptDocs), values);
        }

        internal class ValueSourceFilteredDocIdSet : FilteredDocIdSet
        {
            private readonly ValueSourceFilter outerInstance;
            private readonly FunctionValues values;

            public ValueSourceFilteredDocIdSet(ValueSourceFilter outerInstance, DocIdSet innerSet, FunctionValues values)
                : base(innerSet)
            {
                this.outerInstance = outerInstance;
                this.values = values;
            }

            protected override bool Match(int doc)
            {
                double val = values.DoubleVal(doc);
                return val >= outerInstance.min && val <= outerInstance.max;
            }
        }
    }
}
