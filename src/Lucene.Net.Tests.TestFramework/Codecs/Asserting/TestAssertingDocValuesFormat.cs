﻿// Lucene version compatibility level 8.2.0
using Lucene.Net.Index;
using Lucene.Net.TestFramework;

namespace Lucene.Net.Codecs.Asserting
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
    /// Test <see cref="AssertingDocValuesFormat"/> directly
    /// </summary>
#if TESTFRAMEWORK_MSTEST
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute]
#endif
    public class TestAssertingDocValuesFormat : BaseDocValuesFormatTestCase
#if TESTFRAMEWORK_XUNIT
        , Xunit.IClassFixture<BeforeAfterClass>
    {
        public TestAssertingDocValuesFormat(BeforeAfterClass beforeAfter)
            : base(beforeAfter)
        {
        }
#else
    {
#endif
        // LUCENENET TODO: MSTest is seemingly being fixed to deal with initialization with inheritance for version 2.0. See: https://github.com/microsoft/testfx/issues/143

//#if TESTFRAMEWORK_MSTEST
//        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute]
//        new public static void BeforeClass(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext context)
//        {
//            Lucene.Net.Util.LuceneTestCase.BeforeClass(context);
//        }

//        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute]
//        new public static void AfterClass()
//        {
//            Lucene.Net.Util.LuceneTestCase.AfterClass();
//        }
//#endif

        private readonly Codec codec = new AssertingCodec();
        protected override Codec GetCodec()
        {
            return codec;
        }
    }
}
