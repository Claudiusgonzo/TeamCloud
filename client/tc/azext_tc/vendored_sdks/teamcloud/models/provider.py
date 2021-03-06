# coding=utf-8
# --------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator.
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.
# --------------------------------------------------------------------------

from msrest.serialization import Model


class Provider(Model):
    """Provider.

    :param id:
    :type id: str
    :param url:
    :type url: str
    :param auth_code:
    :type auth_code: str
    :param principal_id:
    :type principal_id: str
    :param version:
    :type version: str
    :param resource_group:
    :type resource_group: ~teamcloud.models.AzureResourceGroup
    :param events:
    :type events: list[str]
    :param properties:
    :type properties: dict[str, str]
    :param registered:
    :type registered: datetime
    :param command_mode: Possible values include: 'Simple', 'Extended'
    :type command_mode: str or ~teamcloud.models.enum
    """

    _attribute_map = {
        'id': {'key': 'id', 'type': 'str'},
        'url': {'key': 'url', 'type': 'str'},
        'auth_code': {'key': 'authCode', 'type': 'str'},
        'principal_id': {'key': 'principalId', 'type': 'str'},
        'version': {'key': 'version', 'type': 'str'},
        'resource_group': {'key': 'resourceGroup', 'type': 'AzureResourceGroup'},
        'events': {'key': 'events', 'type': '[str]'},
        'properties': {'key': 'properties', 'type': '{str}'},
        'registered': {'key': 'registered', 'type': 'iso-8601'},
        'command_mode': {'key': 'commandMode', 'type': 'str'},
    }

    def __init__(self, **kwargs):
        super(Provider, self).__init__(**kwargs)
        self.id = kwargs.get('id', None)
        self.url = kwargs.get('url', None)
        self.auth_code = kwargs.get('auth_code', None)
        self.principal_id = kwargs.get('principal_id', None)
        self.version = kwargs.get('version', None)
        self.resource_group = kwargs.get('resource_group', None)
        self.events = kwargs.get('events', None)
        self.properties = kwargs.get('properties', None)
        self.registered = kwargs.get('registered', None)
        self.command_mode = kwargs.get('command_mode', None)
