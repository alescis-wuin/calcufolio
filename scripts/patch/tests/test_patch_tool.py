from __future__ import annotations

import importlib.util
import json
import tempfile
import unittest
import zipfile
from pathlib import Path

MODULE_PATH = Path(__file__).resolve().parents[1] / "patch_tool.py"
SPEC = importlib.util.spec_from_file_location("patch_tool", MODULE_PATH)
assert SPEC and SPEC.loader
patch_tool = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(patch_tool)


class PatchToolTests(unittest.TestCase):
    def valid_manifest(self) -> dict:
        return {
            "schema_version": 1,
            "patch": {"name": "calcufolio-example", "description": "Example."},
            "repository": {
                "project": "calcufolio",
                "root_name": "calcufolio",
                "start_branch": "develop",
                "end_branch": "feature/example",
                "base_ref": "origin/develop",
                "branch": {
                    "create": True,
                    "type": "feature",
                    "name": "example",
                    "prefix": None,
                    "existing_policy": "fail",
                },
            },
            "apply": {"entrypoint": "apply.sh"},
            "package": {
                "max_uncompressed_bytes": 100000,
                "files": [
                    {
                        "path": "apply.sh",
                        "kind": "shell",
                        "non_empty": True,
                        "sha256": "0" * 64,
                    }
                ],
            },
            "preconditions": {
                "status": {"mode": "exact", "expected_count": 0, "entries": []},
                "required_paths": ["Makefile"],
            },
            "postconditions": {
                "status": {"mode": "exact", "expected_count": 1, "entries": ["?? file"]},
                "changed_paths_mode": "exact",
                "changed_paths": ["file"],
            },
            "validation": {
                "run_all": True,
                "commands": [{"name": "build", "command": "make build"}],
            },
            "manual_test": {
                "mode": "never",
                "prompt": "",
                "command": "",
                "acceptance_prompt": "",
            },
            "stage": {"paths": ["file"]},
            "commit": {
                "enabled": True,
                "sign": True,
                "mode": "automatic",
                "header": "feature(example): add example",
                "body": ["- add example"],
                "footer": "Ticket: TEST-001",
            },
            "post_commit": {"commands": []},
        }

    def test_valid_manifest_is_accepted(self) -> None:
        patch_tool.validate_manifest(self.valid_manifest())

    def test_commit_scope_must_match_branch(self) -> None:
        manifest = self.valid_manifest()
        manifest["commit"]["header"] = "feature(other): add example"
        with self.assertRaises(patch_tool.PatchValidationError):
            patch_tool.validate_manifest(manifest)

    def test_safe_extract_rejects_traversal(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            archive = root / "unsafe.zip"
            with zipfile.ZipFile(archive, "w") as bundle:
                bundle.writestr("package/../escape.txt", "bad")
            with self.assertRaises(patch_tool.PatchValidationError):
                patch_tool.safe_extract(archive, root / "output")

    def test_status_path_parsing(self) -> None:
        self.assertEqual(
            ["a.txt", "new.txt"],
            patch_tool.status_paths([" M a.txt", "R  old.txt -> new.txt"]),
        )

    def test_commit_rendering(self) -> None:
        message = patch_tool.render_commit(self.valid_manifest())
        self.assertIn("feature(example): add example", message)
        self.assertTrue(message.endswith("Ticket: TEST-001\n"))


def test_make_patch_uses_ignored_runtime_runner_copy(self) -> None:
    repository_root = Path(__file__).resolve().parents[3]
    makefile = (repository_root / "Makefile").read_text(encoding="utf-8")
    gitignore = (repository_root / ".gitignore").read_text(encoding="utf-8")

    self.assertIn(
        'runtime_runner="$(PATCH_RUNNER).runtime.$$$$.sh"',
        makefile,
    )
    self.assertIn(
        '"$$runtime_runner"',
        makefile,
    )
    self.assertIn(
        "scripts/patch/apply-package.sh.runtime.*.sh",
        gitignore,
    )

if __name__ == "__main__":
    unittest.main()
