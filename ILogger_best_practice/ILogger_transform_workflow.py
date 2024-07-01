"""
Sample showcasing the ILogger transformation workflow using
iterative planning and a knowledge base.
"""

import json
import os
import sys

repo_dir = f"{os.path.dirname(os.path.realpath(__file__))}/../../../../"
sys.path.append(f"{repo_dir}")

from bandish import Bandish
from bandish.bandish_output import BandishOutput

TEST_PATH = os.path.dirname(os.path.realpath(__file__))


def ILogger_transformation():
    """
    ILogger transformation scenario with knowledge base and iterative planner.
    """
    workflow_content = open(
        os.path.join(TEST_PATH, "workflows", "ILogger_transform_workflow.yaml"), "r", encoding="utf-8"
    ).read()
    params = {
        "organization": "observability",
        "scenario": "ILogger",
        "task": "workflow",
        "task_source": "local",
        "transform_local_dir": f"{TEST_PATH}/input",
        "kb_local_dir": f"{TEST_PATH}/kb",
        "workflow_content": workflow_content,
        "openai_api_key": os.environ["openai_api_key"],
        "openai_api_base": "https://yunlaitest.openai.azure.com/",
        "openai_api_version": "2023-03-15-preview",
        "openai_api_type": "azure",
        "language_model": "gpt-4",
        "output_mode": "return",
    }

    output_json = Bandish.run(params)
    output = BandishOutput(**json.loads(output_json))
    print(output.output_info)


def main() -> None:
    """
    Invoke the entry point for ILogger transformation workflow scenario.
    """
    ILogger_transformation()


if __name__ == "__main__":
    main()
