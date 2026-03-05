|              |                                                       |
| ------------ | ----------------------------------------------------- |
| Copyright    | © VEXIT ® 2025 , www.vexit.com , Tomorrow is today... |
| Author       | Vex Tatarevic                                         |
| Date Created | 2026-02-07                                            |
| Date Updated | 2026-02-07 - Added FlowBase vision documentation      |


FlowBase.json 

```json
{
  "metadata": {
    "version": "1.0",
    "stateStoreType": "File",
    "stateStoreLocation": "Remote",
    "host": "vxs.example.com",
    "stateStoreBasePath": "~/.vexit/apps/vxs/flow-state"
  },
  "flows": {
    "vxs.server.setup": {
      "name": "Server Setup",
      "description": "Complete server provisioning workflow",
      "activities": [
        { "id": "configure.storage", "type": "step" },
        { "id": "phase.core_system_setup", "type": "group" }
      ]
    },
    "vxs.domain.create": {
      "name": "Domain Creation",
      "description": "Create new domain with DNS setup",
      "activities": [
        // ... activity definitions
      ]
    }
  }
}
```





---
*© VEXIT ® 2026 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*