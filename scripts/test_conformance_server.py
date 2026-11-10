"""Isolated contracts for the Python helpers embedded in the server-lane script."""

import ast
import os
from pathlib import Path
import unittest
import urllib.parse
from unittest.mock import patch

SCRIPT = Path(__file__).with_name("conformance-server.sh")
source = SCRIPT.read_text().split("<<'PY'\n", 1)[1].rsplit("\nPY", 1)[0]
# The route and body tables the helpers read, by name.
TABLES = {"CTX_PATHS", "CTX_BODIES", "SCHEME_METHODS"}
functions = [
    node
    for node in ast.parse(source).body
    if isinstance(node, ast.FunctionDef)
    or (
        isinstance(node, ast.Assign)
        and any(isinstance(t, ast.Name) and t.id in TABLES for t in node.targets)
    )
]
helpers = {"os": os, "urllib": urllib}
# Evaluate only local helper definitions and their tables, excluding the
# network-driving script body.
exec(compile(ast.Module(body=functions, type_ignores=[]), str(SCRIPT), "exec"), helpers)  # noqa: S102


class ServerConformanceHelpers(unittest.TestCase):
    def test_numbers_do_not_accept_booleans(self):
        self.assertFalse(helpers["subset_match"](True, 1))
        self.assertFalse(helpers["subset_match"](1, True))
        self.assertTrue(
            helpers["subset_match"]({"value": 2, "extra": True}, {"value": 2})
        )

    def test_signed_credentials_are_headers_not_request_body_fields(self):
        with patch.dict(
            os.environ,
            {
                "EZC_SMOKE_ACCESS_TOKEN": "caller-test-carrier",
                "EZC_SMOKE_CONTEXT_TOKEN": "context-test-carrier",
            },
            clear=True,
        ):
            self.assertEqual(
                helpers["request_headers"](),
                {
                    "Content-Type": "application/json",
                    "Authorization": "Bearer caller-test-carrier",
                    "x-ezclassify-context-token": "context-test-carrier",
                },
            )
        with patch.dict(os.environ, {}, clear=True):
            self.assertEqual(
                helpers["request_headers"](), {"Content-Type": "application/json"}
            )

    def test_context_operation_returns_its_result(self):
        result = {"valid": False}
        self.assertIs(
            helpers["response_value"]({"result": result}, "ctxProcess"), result
        )
        with self.assertRaises(ValueError):
            helpers["response_value"]({"valid": True}, "ctxProcess")

    def test_scan_summary_matches_the_recorded_plan_contract(self):
        self.assertEqual(
            helpers["response_value"](
                {
                    "result": {
                        "results": [
                            {
                                "marking": "High",
                                "start": 0,
                                "result": {"rendered": "HIGH"},
                            }
                        ]
                    }
                },
                "ctxScan",
            ),
            {
                "count": 1,
                "firstMarking": "High",
                "firstStart": 0,
                "firstRendered": "HIGH",
            },
        )
        self.assertEqual(
            helpers["response_value"]({"result": {"results": []}}, "ctxScan")["count"],
            0,
        )
        with self.assertRaises(ValueError):
            helpers["response_value"]({"result": {}}, "ctxScan")

    def test_a_selected_scheme_travels_as_its_query_parameter(self):
        case = {"method": "ctxValidate", "ctxId": "ref-ctx-2", "ctxVersion": "1.0.0"}
        self.assertEqual(
            helpers["case_path"](case), "/v1/ctx/ref-ctx-2/1.0.0/scheme/validate"
        )
        self.assertEqual(
            helpers["case_path"]({**case, "scheme": "ref scheme/2"}),
            "/v1/ctx/ref-ctx-2/1.0.0/scheme/validate?scheme=ref+scheme%2F2",
        )
        with self.assertRaises(ValueError):
            helpers["case_path"](
                {"method": "ctxDocumentAudit", "ctxId": "c", "ctxVersion": "1.0.0", "scheme": "s"}
            )

    def test_every_plan_method_has_a_route_and_a_body(self):
        self.assertEqual(set(helpers["CTX_PATHS"]), set(helpers["CTX_BODIES"]))
        self.assertEqual(
            helpers["CTX_BODIES"]["ctxAggregate"]([["HIGH", "LOW"]]),
            {"inputs": ["HIGH", "LOW"]},
        )
        self.assertEqual(helpers["CTX_BODIES"]["ctxValidate"](["MED"]), {"input": "MED"})


if __name__ == "__main__":
    unittest.main()
