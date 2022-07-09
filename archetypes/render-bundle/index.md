---
title: "REPLACE_TITLE"
date: {{ .Date }}
postcard: "{{ replace (title (after 11 .Name)) "-" "" }}_postcard"
slug: "{{ lower (after 11 .Name) }}"
---

hrender