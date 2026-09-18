// Notification Platform Frontend App Engine
let currentTab = 'dashboard';
let currentChannelModalId = null;

document.addEventListener('DOMContentLoaded', () => {
    initNavigation();
    loadDashboardStats();
});

function initNavigation() {
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const tab = item.getAttribute('data-tab');
            switchTab(tab);
        });
    });
}

function switchTab(tab) {
    currentTab = tab;
    document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
    document.querySelectorAll('.tab-pane').forEach(el => el.classList.remove('active'));

    const activeNav = document.querySelector(`.nav-item[data-tab="${tab}"]`);
    if (activeNav) activeNav.classList.add('active');

    const activePane = document.getElementById(`tab-${tab}`);
    if (activePane) activePane.classList.add('active');

    updateHeaderTitle(tab);
    refreshCurrentTab();
}

function updateHeaderTitle(tab) {
    const titleEl = document.getElementById('page-title');
    const subEl = document.getElementById('page-subtitle');
    const btnText = document.getElementById('action-btn-text');

    const titles = {
        dashboard: { title: 'Dashboard General', sub: 'Métricas, estado de Quartz.NET y actividad reciente', btn: 'Nuevo Job' },
        jobs: { title: 'Eventos y Jobs', sub: 'Gestión de ejecuciones únicas y recurrentes (Cron)', btn: 'Nuevo Job' },
        channels: { title: 'Canales de Notificación', sub: 'Configuración de servidores SMTP y Webhooks', btn: 'Nuevo Canal' },
        datasources: { title: 'Fuentes de Datos', sub: 'Conectores SQL Server, SQLite, MySQL, REST APIs', btn: 'Nueva Fuente' },
        templates: { title: 'Plantillas Dinámicas', sub: 'Renderizado de HTML/Texto con Fluid (Liquid)', btn: 'Nueva Plantilla' },
        conditions: { title: 'Reglas Condicionales', sub: 'Condiciones dinámicas previa ejecución del Job', btn: 'Nueva Regla' },
        postactions: { title: 'Acciones Post-Ejecución', sub: 'Consultas SQL tras el envío exitoso', btn: 'Nueva Acción' },
        logs: { title: 'Historial & Bitácora', sub: 'Logs detallados de ejecuciones y auditoría', btn: 'Actualizar' }
    };

    if (titles[tab]) {
        titleEl.textContent = titles[tab].title;
        subEl.textContent = titles[tab].sub;
        if (btnText) btnText.textContent = titles[tab].btn;
    }
}

function refreshCurrentTab() {
    switch (currentTab) {
        case 'dashboard': loadDashboardStats(); break;
        case 'jobs': loadJobsTable(); break;
        case 'channels': loadChannelsCards(); break;
        case 'datasources': loadDataSourcesCards(); break;
        case 'templates': loadTemplatesCards(); break;
        case 'conditions': loadConditionsTable(); break;
        case 'postactions': loadPostActionsTable(); break;
        case 'logs': loadLogsTable(); break;
    }
}

function openCreateModal() {
    switch (currentTab) {
        case 'dashboard':
        case 'jobs': openJobModal(); break;
        case 'channels': openChannelModal(); break;
        case 'datasources': openDataSourceModal(); break;
        case 'templates': openTemplateModal(); break;
        case 'conditions': openConditionModal(); break;
        case 'postactions': openPostActionModal(); break;
        case 'logs': loadLogsTable(); break;
    }
}

// -------------------------------------------------------------
// 1. DASHBOARD & STATS
// -------------------------------------------------------------
async function loadDashboardStats() {
    try {
        const res = await fetch('/api/dashboard');
        const data = await res.json();

        document.getElementById('stat-total-jobs').textContent = data.totalJobs;
        document.getElementById('stat-active-jobs').textContent = `${data.activeJobs} activos`;
        document.getElementById('stat-success-logs').textContent = data.successLogs;
        document.getElementById('stat-skipped-logs').textContent = data.skippedLogs;
        document.getElementById('stat-failed-logs').textContent = data.failedLogs;

        renderDashboardRecentTable(data.recentLogs);
    } catch (err) {
        console.error("Error al cargar dashboard:", err);
    }
}

function renderDashboardRecentTable(logs) {
    const tbody = document.getElementById('dashboard-recent-table');
    if (!logs || logs.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center">No hay actividad reciente registrada.</td></tr>`;
        return;
    }

    tbody.innerHTML = logs.map(l => `
        <tr>
            <td>${new Date(l.triggeredAt).toLocaleString()}</td>
            <td><strong>${escapeHtml(l.jobName)}</strong></td>
            <td>${getStatusBadge(l.status)}</td>
            <td>${escapeHtml(l.targetRecipient || '-')}</td>
            <td>${l.dataRowsFetched} filas</td>
            <td>${l.durationMs} ms</td>
            <td>
                <button class="btn btn-sm btn-secondary" onclick="viewLogDetail(${l.id})">
                    <i class="fa-solid fa-eye"></i> Detalle
                </button>
            </td>
        </tr>
    `).join('');
}

function getStatusBadge(status) {
    switch (status) {
        case 1: return `<span class="badge badge-success"><i class="fa-solid fa-check"></i> Enviado</span>`;
        case 2: return `<span class="badge badge-warning"><i class="fa-solid fa-forward"></i> Omitido</span>`;
        case 3: return `<span class="badge badge-danger"><i class="fa-solid fa-xmark"></i> Fallido</span>`;
        default: return `<span class="badge badge-info">Desconocido</span>`;
    }
}

// -------------------------------------------------------------
// 2. JOBS MANAGEMENT
// -------------------------------------------------------------
async function loadJobsTable() {
    try {
        const res = await fetch('/api/jobs');
        const jobs = await res.json();
        const tbody = document.getElementById('jobs-table-body');

        if (!jobs || jobs.length === 0) {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center">No hay Jobs registrados aún.</td></tr>`;
            return;
        }

        tbody.innerHTML = jobs.map(j => `
            <tr>
                <td>
                    <button class="btn btn-sm ${j.isActive ? 'btn-primary' : 'btn-secondary'}" onclick="toggleJobActive(${j.id})">
                        ${j.isActive ? 'Activo' : 'Inactivo'}
                    </button>
                </td>
                <td>
                    <strong>${escapeHtml(j.name)}</strong>
                    <div style="font-size:12px; color:var(--text-muted);">${escapeHtml(j.description || '')}</div>
                </td>
                <td>
                    ${j.scheduleType === 2 ? `<span class="badge badge-info"><i class="fa-solid fa-rotate"></i> Cron: ${escapeHtml(j.cronExpression || '')}</span>` : `<span class="badge badge-warning"><i class="fa-solid fa-bolt"></i> Único</span>`}
                </td>
                <td>${j.channel ? escapeHtml(j.channel.name) : '-'}</td>
                <td>${j.template ? escapeHtml(j.template.name) : '-'}</td>
                <td>${j.conditionalRule ? escapeHtml(j.conditionalRule.name) : '<span style="color:var(--text-dim)">Ninguno</span>'}</td>
                <td>${j.lastRunAt ? new Date(j.lastRunAt).toLocaleString() : 'Nunca'}</td>
                <td>
                    <div style="display:flex; gap:6px;">
                        <button class="btn btn-sm btn-primary" onclick="triggerJobNow(${j.id})">
                            <i class="fa-solid fa-play"></i> Ejecutar Ahora
                        </button>
                        <button class="btn btn-sm btn-secondary" onclick="editJob(${j.id})"><i class="fa-solid fa-pen"></i></button>
                        <button class="btn btn-sm btn-danger" onclick="deleteJob(${j.id})"><i class="fa-solid fa-trash"></i></button>
                    </div>
                </td>
            </tr>
        `).join('');
    } catch (err) {
        console.error("Error al cargar jobs:", err);
    }
}

async function triggerJobNow(id) {
    showModal('Ejecutando Job...', `<div class="text-center"><i class="fa-solid fa-spinner fa-spin fa-2x"></i><p style="margin-top:10px;">Ejecutando consulta, condicional y canal...</p></div>`, false);
    try {
        const res = await fetch(`/api/jobs/${id}/trigger`, { method: 'POST' });
        const log = await res.json();
        
        let content = `
            <div>
                <h4>Estado: ${getStatusBadge(log.status)}</h4>
                <p><strong>Job:</strong> ${escapeHtml(log.jobName)}</p>
                <p><strong>Destinatario:</strong> ${escapeHtml(log.targetRecipient || 'N/A')}</p>
                <p><strong>Filas Obtenidas:</strong> ${log.dataRowsFetched}</p>
                <p><strong>Duración:</strong> ${log.durationMs} ms</p>
                ${log.errorMessage ? `<div style="background:rgba(239,68,68,0.15); color:var(--accent-red); padding:10px; border-radius:6px; margin-top:10px;">${escapeHtml(log.errorMessage)}</div>` : ''}
                ${log.renderedBody ? `<div style="margin-top:15px; background:rgba(0,0,0,0.3); padding:12px; border-radius:6px;"><strong>Previsualización Cuerpo:</strong><div style="margin-top:5px; font-size:13px;">${log.renderedBody}</div></div>` : ''}
            </div>
        `;
        showModal('Resultado de Ejecución Inmediata', content, true);
        loadDashboardStats();
    } catch (err) {
        showModal('Error', `<p>Error al ejecutar el job: ${err.message}</p>`, true);
    }
}

async function toggleJobActive(id) {
    await fetch(`/api/jobs/${id}/toggle`, { method: 'POST' });
    loadJobsTable();
}

async function openJobModal(jobId = null) {
    // Fetch channels, templates, datasources, conditions, postactions
    const [chRes, tRes, dsRes, cRes, pRes] = await Promise.all([
        fetch('/api/channels'), fetch('/api/templates'), fetch('/api/datasources'), fetch('/api/conditions'), fetch('/api/postactions')
    ]);

    const channels = await chRes.json();
    const templates = await tRes.json();
    const dataSources = await dsRes.json();
    const conditions = await cRes.json();
    const postActions = await pRes.json();

    let job = { name: '', description: '', scheduleType: 2, cronExpression: '0 0/5 * * * ?', recipientExpression: 'demo@empresa.com', isActive: true };
    if (jobId) {
        const jRes = await fetch(`/api/jobs/${jobId}`);
        job = await jRes.json();
    }

    const html = `
        <form id="job-form">
            <div class="form-group">
                <label>Nombre del Evento / Job *</label>
                <input type="text" id="j-name" class="form-control" value="${escapeHtml(job.name)}" required>
            </div>
            <div class="form-group">
                <label>Descripción</label>
                <input type="text" id="j-desc" class="form-control" value="${escapeHtml(job.description || '')}">
            </div>
            <div style="display:flex; gap:16px;">
                <div class="form-group" style="flex:1;">
                    <label>Tipo Programación</label>
                    <select id="j-type" class="form-control" onchange="toggleScheduleFields()">
                        <option value="2" ${job.scheduleType === 2 ? 'selected' : ''}>Recurrente (Cron)</option>
                        <option value="1" ${job.scheduleType === 1 ? 'selected' : ''}>Ejecución Única</option>
                    </select>
                </div>
                <div class="form-group" style="flex:2;" id="cron-group">
                    <label>Expresión Cron (Quartz)</label>
                    <input type="text" id="j-cron" class="form-control" value="${escapeHtml(job.cronExpression || '0 0/5 * * * ?')}">
                </div>
            </div>
            <div style="display:flex; gap:16px;">
                <div class="form-group" style="flex:1;">
                    <label>Canal Notificación *</label>
                    <select id="j-channel" class="form-control" required>
                        ${channels.map(c => `<option value="${c.id}" ${job.channelId === c.id ? 'selected' : ''}>${escapeHtml(c.name)} (${c.type === 1 ? 'Email' : 'Webhook'})</option>`).join('')}
                    </select>
                </div>
                <div class="form-group" style="flex:1;">
                    <label>Plantilla *</label>
                    <select id="j-template" class="form-control" required>
                        ${templates.map(t => `<option value="${t.id}" ${job.templateId === t.id ? 'selected' : ''}>${escapeHtml(t.name)}</option>`).join('')}
                    </select>
                </div>
            </div>
            <div class="form-group">
                <label>Destinatario (Email/URL o Expresión Liquid) *</label>
                <input type="text" id="j-recipient" class="form-control" value="${escapeHtml(job.recipientExpression)}" required placeholder="ej. usuario@dominio.com o {{ item.email }}">
            </div>
            <div class="form-group">
                <label>Fuente de Datos Extractor (Opcional)</label>
                <select id="j-ds" class="form-control">
                    <option value="">-- Sin fuente de datos --</option>
                    ${dataSources.map(d => `<option value="${d.id}" ${job.dataSourceId === d.id ? 'selected' : ''}>${escapeHtml(d.name)}</option>`).join('')}
                </select>
            </div>
            <div class="form-group">
                <label>Consulta SQL / Endpoint Extractor (Opcional)</label>
                <textarea id="j-query" class="form-control" placeholder="SELECT * FROM NotificacionesPendientes WHERE Estado = 'PENDIENTE'">${escapeHtml(job.dataQuery || '')}</textarea>
            </div>
            <div style="display:flex; gap:16px;">
                <div class="form-group" style="flex:1;">
                    <label>Regla Condicional (Opcional)</label>
                    <select id="j-condition" class="form-control">
                        <option value="">-- Sin condición --</option>
                        ${conditions.map(c => `<option value="${c.id}" ${job.conditionalRuleId === c.id ? 'selected' : ''}>${escapeHtml(c.name)}</option>`).join('')}
                    </select>
                </div>
                <div class="form-group" style="flex:1;">
                    <label>Acción Post-Ejecución (Opcional)</label>
                    <select id="j-postaction" class="form-control">
                        <option value="">-- Sin acción posterior --</option>
                        ${postActions.map(p => `<option value="${p.id}" ${job.postExecutionActionId === p.id ? 'selected' : ''}>${escapeHtml(p.name)}</option>`).join('')}
                    </select>
                </div>
            </div>
        </form>
    `;

    showModal(jobId ? 'Editar Job' : 'Nuevo Job de Notificación', html, true, async () => {
        const payload = {
            name: document.getElementById('j-name').value,
            description: document.getElementById('j-desc').value,
            scheduleType: parseInt(document.getElementById('j-type').value),
            cronExpression: document.getElementById('j-cron').value,
            channelId: parseInt(document.getElementById('j-channel').value),
            templateId: parseInt(document.getElementById('j-template').value),
            recipientExpression: document.getElementById('j-recipient').value,
            dataSourceId: document.getElementById('j-ds').value ? parseInt(document.getElementById('j-ds').value) : null,
            dataQuery: document.getElementById('j-query').value,
            conditionalRuleId: document.getElementById('j-condition').value ? parseInt(document.getElementById('j-condition').value) : null,
            postExecutionActionId: document.getElementById('j-postaction').value ? parseInt(document.getElementById('j-postaction').value) : null,
            isActive: true
        };

        const url = jobId ? `/api/jobs/${jobId}` : '/api/jobs';
        const method = jobId ? 'PUT' : 'POST';

        await fetch(url, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        closeModal();
        loadJobsTable();
    });
}

async function deleteJob(id) {
    if (confirm("¿Deseas eliminar este Job programado?")) {
        await fetch(`/api/jobs/${id}`, { method: 'DELETE' });
        loadJobsTable();
    }
}

// -------------------------------------------------------------
// 3. CHANNELS
// -------------------------------------------------------------
async function loadChannelsCards() {
    const res = await fetch('/api/channels');
    const channels = await res.json();
    const container = document.getElementById('channels-cards-grid');

    container.innerHTML = channels.map(c => `
        <div class="item-card">
            <div>
                <div class="item-card-header">
                    <span class="item-card-title">${escapeHtml(c.name)}</span>
                    <span class="badge badge-info">${escapeHtml(getChannelTypeName(c.type, c.channelTypeId))}</span>
                </div>
                <div class="channel-card-meta">
                    <span><i class="fa-solid fa-sliders"></i> Configuración relacional</span>
                    <span class="${c.isActive ? 'text-success' : 'text-muted'}"><i class="fa-solid fa-circle"></i> ${c.isActive ? 'Activo' : 'Inactivo'}</span>
                </div>
            </div>
            <div class="item-card-actions">
                <button class="btn btn-sm btn-secondary" onclick="openChannelModal(${c.id})"><i class="fa-solid fa-gear"></i> Configurar</button>
                <button class="btn btn-sm btn-primary" onclick="testChannelModal(${c.id})"><i class="fa-solid fa-paper-plane"></i> Probar</button>
                <button class="btn btn-sm btn-danger" onclick="deleteChannel(${c.id})"><i class="fa-solid fa-trash"></i></button>
            </div>
        </div>
    `).join('');
}

async function openChannelModal(channelId = null) {
    currentChannelModalId = channelId;
    const types = await (await fetch('/api/channels/types')).json();
    const channel = channelId ? await (await fetch(`/api/channels/${channelId}`)).json() : null;
    const selectedTypeId = channel?.channelTypeId || types[0]?.id;
    const definitions = selectedTypeId ? await (await fetch(`/api/channels/types/${selectedTypeId}/definitions`)).json() : [];
    const currentSettings = Object.fromEntries((channel?.settings || []).map(setting => [setting.code, setting]));

    const html = `
        <form>
            <div class="form-group">
                <label>Nombre del Canal *</label>
                <input type="text" id="c-name" class="form-control" required value="${escapeHtml(channel?.name || '')}" placeholder="ej. Servidor Email Corporativo">
            </div>
            <div class="form-group">
                <label>Tipo de Canal *</label>
                <select id="c-type" class="form-control" onchange="loadChannelDefinitionEditor()" ${channelId ? 'disabled' : ''}>
                    ${types.map(type => `<option value="${type.id}" ${type.id === selectedTypeId ? 'selected' : ''}>${escapeHtml(type.name)} (${escapeHtml(type.code)})</option>`).join('')}
                </select>
            </div>
            <div class="channel-settings-heading">
                <div><strong>Parámetros del canal</strong><span>Los secretos existentes no se muestran.</span></div>
                <button type="button" class="btn btn-sm btn-secondary" onclick="validateChannelForm()"><i class="fa-solid fa-shield-halved"></i> Validar</button>
            </div>
            <div id="channel-definition-editor">${renderChannelDefinitions(definitions, currentSettings)}</div>
        </form>
    `;
    showModal(channelId ? 'Editar Canal de Notificación' : 'Nuevo Canal de Notificación', html, true, () => saveChannel(channelId));
}

function renderChannelDefinitions(definitions, currentSettings = {}) {
    if (!definitions.length) return '<div class="empty-state">Este tipo todavía no tiene parámetros definidos.</div>';
    return definitions.map(definition => {
        const setting = currentSettings[definition.code];
        const value = setting?.value ?? definition.defaultValue ?? '';
        const required = definition.isRequired ? 'required' : '';
        if (definition.isSensitive) {
            return `<div class="channel-setting-row sensitive-setting">
                <div><label>${escapeHtml(definition.name)} ${definition.isRequired ? '*' : ''}</label><small>${escapeHtml(definition.code)} · Secreto cifrado</small></div>
                <div class="secret-control"><input type="password" class="form-control channel-setting-input" data-code="${escapeHtml(definition.code)}" data-type="${escapeHtml(definition.dataType)}" placeholder="${setting?.isConfigured ? 'Configurado · dejar vacío para conservar' : 'Introducir secreto'}"><label class="checkbox-label"><input type="checkbox" data-delete-code="${escapeHtml(definition.code)}"> Eliminar</label></div>
            </div>`;
        }
        return `<div class="channel-setting-row"><div><label>${escapeHtml(definition.name)} ${definition.isRequired ? '*' : ''}</label><small>${escapeHtml(definition.code)} · ${escapeHtml(definition.dataType)}</small></div>${channelInput(definition, value, required)}</div>`;
    }).join('');
}

function channelInput(definition, value, required) {
    const code = escapeHtml(definition.code);
    const type = escapeHtml(definition.dataType);
    if (definition.dataType === 'BOOL') return `<label class="toggle-control"><input type="checkbox" class="channel-setting-input" data-code="${code}" data-type="${type}" ${value === true || value === 'true' ? 'checked' : ''}><span>Activado</span></label>`;
    const inputType = definition.dataType === 'INT' || definition.dataType === 'DECIMAL' ? 'number' : 'text';
    return `<input type="${inputType}" class="form-control channel-setting-input" data-code="${code}" data-type="${type}" value="${escapeHtml(value)}" ${required}>`;
}

async function loadChannelDefinitionEditor() {
    const typeId = document.getElementById('c-type').value;
    const definitions = await (await fetch(`/api/channels/types/${typeId}/definitions`)).json();
    document.getElementById('channel-definition-editor').innerHTML = renderChannelDefinitions(definitions);
}

async function saveChannel(channelId) {
    const typeId = parseInt(document.getElementById('c-type').value);
    const type = channelEnumForType(typeId);
    const payload = { name: document.getElementById('c-name').value, channelTypeId: typeId, type, isActive: true };
    const response = await fetch(channelId ? `/api/channels/${channelId}` : '/api/channels', { method: channelId ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
    if (!response.ok) { alert(await response.text()); return; }
    const saved = await response.json();
    const targetId = channelId || saved.id;
    for (const input of document.querySelectorAll('.channel-setting-input')) {
        const code = input.dataset.code;
        const deleteInput = document.querySelector(`[data-delete-code="${code}"]`);
        if (deleteInput?.checked) {
            await fetch(`/api/channels/${targetId}/settings/${encodeURIComponent(code)}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ delete: true }) });
            continue;
        }
        if (input.type === 'password' && !input.value) continue;
        const value = input.type === 'checkbox' ? input.checked : input.value;
        if (value === '') continue;
        await fetch(`/api/channels/${targetId}/settings/${encodeURIComponent(code)}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ value }) });
    }
    closeModal();
    loadChannelsCards();
}

async function validateChannelForm() {
    const id = currentChannelModalId;
    if (!id) { alert('Guarda el canal antes de ejecutar la validación completa.'); return; }
    const result = await (await fetch(`/api/channels/${id}/validate`, { method: 'POST' })).json();
    alert(result.valid ? 'La configuración es válida.' : result.errors.join('\n'));
}

function getChannelTypeName(type, channelTypeId) {
    if (channelTypeId === 1 || type === 1) return 'EMAIL';
    if (channelTypeId === 2 || type === 3) return 'SMS';
    if (channelTypeId === 3 || type === 2) return 'WEBHOOK';
    return 'CANAL';
}

function channelEnumForType(channelTypeId) {
    if (channelTypeId === 1) return 1;
    if (channelTypeId === 2) return 3;
    if (channelTypeId === 3) return 2;
    return 1;
}

async function testChannelModal(id) {
    const html = `
        <div>
            <div class="form-group">
                <label>Destinatario de Prueba (Email o URL Webhook) *</label>
                <input type="text" id="test-recip" class="form-control" value="destinatario@demo.com">
            </div>
            <div class="form-group">
                <label>Asunto</label>
                <input type="text" id="test-subj" class="form-control" value="Prueba de Envío Notificación">
            </div>
            <div class="form-group">
                <label>Cuerpo / Payload</label>
                <textarea id="test-body" class="form-control"><h2>Notificación de prueba</h2><p>Este es un mensaje de prueba enviado directamente desde el canal de notificación.</p></textarea>
            </div>
        </div>
    `;
    showModal('Probar Envío por Canal', html, true, async () => {
        const payload = {
            recipient: document.getElementById('test-recip').value,
            subject: document.getElementById('test-subj').value,
            body: document.getElementById('test-body').value
        };
        const res = await fetch(`/api/channels/${id}/test`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        const data = await res.json();
        alert(data.message);
        closeModal();
    });
}

async function deleteChannel(id) {
    if (confirm("¿Eliminar este canal?")) {
        await fetch(`/api/channels/${id}`, { method: 'DELETE' });
        loadChannelsCards();
    }
}

// -------------------------------------------------------------
// 4. DATA SOURCES
// -------------------------------------------------------------
async function loadDataSourcesCards() {
    const res = await fetch('/api/datasources');
    const list = await res.json();
    const container = document.getElementById('datasources-cards-grid');

    container.innerHTML = list.map(d => `
        <div class="item-card">
            <div>
                <div class="item-card-header">
                    <span class="item-card-title">${escapeHtml(d.name)}</span>
                    <span class="badge badge-info">${getDsTypeName(d.type)}</span>
                </div>
                <div class="item-card-body">
                    <code>${escapeHtml(d.connectionStringOrUrl)}</code>
                </div>
            </div>
            <div class="item-card-actions">
                <button class="btn btn-sm btn-primary" onclick="testQueryModal(${d.id})"><i class="fa-solid fa-play"></i> Probar Consulta</button>
                <button class="btn btn-sm btn-danger" onclick="deleteDataSource(${d.id})"><i class="fa-solid fa-trash"></i></button>
            </div>
        </div>
    `).join('');
}

function getDsTypeName(type) {
    switch(type) {
        case 1: return 'SQL Server';
        case 2: return 'SQLite';
        case 3: return 'PostgreSQL';
        case 4: return 'MySQL';
        case 5: return 'REST API';
        default: return 'Desconocido';
    }
}

function openDataSourceModal() {
    const html = `
        <form>
            <div class="form-group">
                <label>Nombre de la Fuente de Datos *</label>
                <input type="text" id="ds-name" class="form-control" required placeholder="ej. DB Ventas Producción">
            </div>
            <div class="form-group">
                <label>Tipo de Motor *</label>
                <select id="ds-type" class="form-control">
                    <option value="2">SQLite</option>
                    <option value="1">SQL Server</option>
                    <option value="3">PostgreSQL</option>
                    <option value="4">MySQL</option>
                    <option value="5">REST API (Endpoint HTTP)</option>
                </select>
            </div>
            <div class="form-group">
                <label>Cadena de Conexión o URL Base *</label>
                <input type="text" id="ds-conn" class="form-control" required value="Data Source=notification_service.db">
            </div>
        </form>
    `;
    showModal('Nueva Fuente de Datos Extractor', html, true, async () => {
        const payload = {
            name: document.getElementById('ds-name').value,
            type: parseInt(document.getElementById('ds-type').value),
            connectionStringOrUrl: document.getElementById('ds-conn').value
        };
        await fetch('/api/datasources', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        closeModal();
        loadDataSourcesCards();
    });
}

async function testQueryModal(dsId) {
    const html = `
        <div>
            <div class="form-group">
                <label>Consulta SQL a Ejecutar o Endpoint *</label>
                <textarea id="tq-query" class="form-control">SELECT * FROM Channels</textarea>
            </div>
            <div id="tq-result" style="margin-top:15px; display:none; max-height:250px; overflow:auto; background:rgba(0,0,0,0.4); padding:12px; border-radius:6px;"></div>
        </div>
    `;
    showModal('Probador de Consulta SQL / API', html, true, async () => {
        const query = document.getElementById('tq-query').value;
        const res = await fetch('/api/datasources/test', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ dataSourceId: dsId, query })
        });
        const data = await res.json();
        
        const resultBox = document.getElementById('tq-result');
        resultBox.style.display = 'block';
        if (data.success) {
            resultBox.innerHTML = `<strong>Éxito! ${data.rowCount} filas devueltas:</strong><pre style="margin-top:5px; font-size:12px;">${JSON.stringify(data.data, null, 2)}</pre>`;
        } else {
            resultBox.innerHTML = `<span style="color:var(--accent-red)">Error: ${escapeHtml(data.error)}</span>`;
        }
    });
}

async function deleteDataSource(id) {
    if (confirm("¿Eliminar esta fuente de datos?")) {
        await fetch(`/api/datasources/${id}`, { method: 'DELETE' });
        loadDataSourcesCards();
    }
}

// -------------------------------------------------------------
// 5. TEMPLATES (FLUID LIQUID)
// -------------------------------------------------------------
async function loadTemplatesCards() {
    const res = await fetch('/api/templates');
    const list = await res.json();
    const container = document.getElementById('templates-cards-grid');

    container.innerHTML = list.map(t => `
        <div class="item-card">
            <div>
                <div class="item-card-header">
                    <span class="item-card-title">${escapeHtml(t.name)}</span>
                    <span class="badge badge-info">${t.channelType === 1 ? 'Email' : 'Webhook'}</span>
                </div>
                <div class="item-card-body">
                    <strong>Asunto:</strong> <code>${escapeHtml(t.subjectTemplate)}</code>
                </div>
            </div>
            <div class="item-card-actions">
                <button class="btn btn-sm btn-primary" onclick="previewTemplateModal(${t.id})"><i class="fa-solid fa-eye"></i> Previsualizar</button>
                <button class="btn btn-sm btn-danger" onclick="deleteTemplate(${t.id})"><i class="fa-solid fa-trash"></i></button>
            </div>
        </div>
    `).join('');
}

function openTemplateModal() {
    const html = `
        <form>
            <div class="form-group">
                <label>Nombre de la Plantilla *</label>
                <input type="text" id="t-name" class="form-control" required placeholder="ej. Plantilla Alerta Facturas">
            </div>
            <div class="form-group">
                <label>Tipo de Canal Destino *</label>
                <select id="t-channel" class="form-control">
                    <option value="1">Email (HTML)</option>
                    <option value="2">Webhook (JSON / Texto)</option>
                </select>
            </div>
            <div class="form-group">
                <label>Plantilla del Asunto (Fluid Liquid) *</label>
                <input type="text" id="t-subject" class="form-control" value="Alerta de {{ total_pendientes }} elementos pendientes">
            </div>
            <div class="form-group">
                <label>Plantilla del Cuerpo (Fluid Liquid HTML/JSON) *</label>
                <textarea id="t-body" class="form-control" style="min-height:140px;"><h2>Hola {{ usuario_nombre }}</h2><p>Tienes {{ total_pendientes }} tareas pendientes.</p><ul>{% for item in items %}<li>{{ item.name }}</li>{% endfor %}</ul></textarea>
            </div>
        </form>
    `;
    showModal('Nueva Plantilla Dinámica', html, true, async () => {
        const payload = {
            name: document.getElementById('t-name').value,
            channelType: parseInt(document.getElementById('t-channel').value),
            subjectTemplate: document.getElementById('t-subject').value,
            bodyTemplate: document.getElementById('t-body').value
        };
        await fetch('/api/templates', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        closeModal();
        loadTemplatesCards();
    });
}

async function previewTemplateModal(id) {
    const res = await fetch(`/api/templates/${id}`);
    const t = await res.json();

    const sampleJson = JSON.stringify({
        usuario_nombre: "Juan Pérez",
        total_pendientes: 3,
        items: [{ name: "Orden #101" }, { name: "Orden #102" }]
    }, null, 2);

    const html = `
        <div>
            <div class="form-group">
                <label>Datos JSON de Prueba Contextual</label>
                <textarea id="prev-json" class="form-control" style="min-height:90px;">${sampleJson}</textarea>
            </div>
            <button class="btn btn-sm btn-secondary" onclick="runLivePreview('${id}')" style="margin-bottom:12px;">Renderizar</button>
            <div id="prev-result" style="background:rgba(0,0,0,0.5); padding:12px; border-radius:6px; border:1px solid var(--border-color);">
                <strong>Asunto:</strong> <div id="prev-res-subj">...</div>
                <hr style="border-color:var(--border-color); margin:8px 0;">
                <strong>Cuerpo HTML:</strong> <div id="prev-res-body">...</div>
            </div>
        </div>
    `;

    showModal(`Previsualizar Plantilla: ${escapeHtml(t.name)}`, html, false);
    window.runLivePreview = async () => {
        const sampleJsonData = document.getElementById('prev-json').value;
        const pRes = await fetch('/api/templates/preview', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ subjectTemplate: t.subjectTemplate, bodyTemplate: t.bodyTemplate, sampleJsonData })
        });
        const pData = await pRes.json();
        if (pData.success) {
            document.getElementById('prev-res-subj').textContent = pData.subject;
            document.getElementById('prev-res-body').innerHTML = pData.body;
        } else {
            alert("Error: " + pData.error);
        }
    };
    window.runLivePreview();
}

async function deleteTemplate(id) {
    if (confirm("¿Eliminar esta plantilla?")) {
        await fetch(`/api/templates/${id}`, { method: 'DELETE' });
        loadTemplatesCards();
    }
}

// -------------------------------------------------------------
// 6. CONDITIONS
// -------------------------------------------------------------
async function loadConditionsTable() {
    const res = await fetch('/api/conditions');
    const list = await res.json();
    const tbody = document.getElementById('conditions-table-body');

    if (!list || list.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center">No hay reglas condicionales registradas.</td></tr>`;
        return;
    }

    tbody.innerHTML = list.map(c => `
        <tr>
            <td><strong>${escapeHtml(c.name)}</strong></td>
            <td>${c.type === 1 ? 'Expresión Liquid' : 'Conteo Filas SQL'}</td>
            <td>${c.dataSource ? escapeHtml(c.dataSource.name) : '-'}</td>
            <td><code>${escapeHtml(c.expression || c.sqlQuery || '')}</code></td>
            <td>${c.expectedMinCount}</td>
            <td>
                <button class="btn btn-sm btn-danger" onclick="deleteCondition(${c.id})"><i class="fa-solid fa-trash"></i></button>
            </td>
        </tr>
    `).join('');
}

async function openConditionModal() {
    const dsRes = await fetch('/api/datasources');
    const dataSources = await dsRes.json();

    const html = `
        <form>
            <div class="form-group">
                <label>Nombre de la Regla Condicional *</label>
                <input type="text" id="cond-name" class="form-control" required placeholder="ej. Ejecutar solo si hay datos">
            </div>
            <div class="form-group">
                <label>Tipo de Validación *</label>
                <select id="cond-type" class="form-control">
                    <option value="1">Expresión Liquid (ej. total_pendientes > 0)</option>
                    <option value="2">Mínimo de Filas SQL (RowCount >= Min)</option>
                </select>
            </div>
            <div class="form-group">
                <label>Expresión Liquid o Condición</label>
                <input type="text" id="cond-expr" class="form-control" value="total_pendientes > 0">
            </div>
            <div class="form-group">
                <label>Fuente de Datos para Consulta Adicional (Opcional)</label>
                <select id="cond-ds" class="form-control">
                    <option value="">-- Usar datos del Job principal --</option>
                    ${dataSources.map(d => `<option value="${d.id}">${escapeHtml(d.name)}</option>`).join('')}
                </select>
            </div>
        </form>
    `;
    showModal('Nueva Regla Condicional', html, true, async () => {
        const payload = {
            name: document.getElementById('cond-name').value,
            type: parseInt(document.getElementById('cond-type').value),
            expression: document.getElementById('cond-expr').value,
            dataSourceId: document.getElementById('cond-ds').value ? parseInt(document.getElementById('cond-ds').value) : null,
            expectedMinCount: 1
        };
        await fetch('/api/conditions', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        closeModal();
        loadConditionsTable();
    });
}

async function deleteCondition(id) {
    if (confirm("¿Eliminar esta regla condicional?")) {
        await fetch(`/api/conditions/${id}`, { method: 'DELETE' });
        loadConditionsTable();
    }
}

// -------------------------------------------------------------
// 7. POST ACTIONS
// -------------------------------------------------------------
async function loadPostActionsTable() {
    const res = await fetch('/api/postactions');
    const list = await res.json();
    const tbody = document.getElementById('postactions-table-body');

    if (!list || list.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center">No hay acciones post-ejecución registradas.</td></tr>`;
        return;
    }

    tbody.innerHTML = list.map(a => `
        <tr>
            <td><strong>${escapeHtml(a.name)}</strong></td>
            <td>${a.dataSource ? escapeHtml(a.dataSource.name) : '-'}</td>
            <td><code>${escapeHtml(a.sqlQuery)}</code></td>
            <td>${new Date(a.createdAt).toLocaleString()}</td>
            <td>
                <button class="btn btn-sm btn-danger" onclick="deletePostAction(${a.id})"><i class="fa-solid fa-trash"></i></button>
            </td>
        </tr>
    `).join('');
}

async function openPostActionModal() {
    const dsRes = await fetch('/api/datasources');
    const dataSources = await dsRes.json();

    const html = `
        <form>
            <div class="form-group">
                <label>Nombre de la Acción Post-Ejecución *</label>
                <input type="text" id="pa-name" class="form-control" required placeholder="ej. Marcar Notificaciones Enviadas">
            </div>
            <div class="form-group">
                <label>Fuente de Datos Destino *</label>
                <select id="pa-ds" class="form-control" required>
                    ${dataSources.map(d => `<option value="${d.id}">${escapeHtml(d.name)}</option>`).join('')}
                </select>
            </div>
            <div class="form-group">
                <label>Consulta SQL a Ejecutar (Soporta sintaxis Liquid) *</label>
                <textarea id="pa-sql" class="form-control" required placeholder="UPDATE Pendientes SET Enviado = 1 WHERE Fecha = CURRENT_DATE"></textarea>
            </div>
        </form>
    `;
    showModal('Nueva Acción Post-Ejecución SQL', html, true, async () => {
        const payload = {
            name: document.getElementById('pa-name').value,
            dataSourceId: parseInt(document.getElementById('pa-ds').value),
            actionType: 1,
            sqlQuery: document.getElementById('pa-sql').value
        };
        await fetch('/api/postactions', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        closeModal();
        loadPostActionsTable();
    });
}

async function deletePostAction(id) {
    if (confirm("¿Eliminar esta acción post-ejecución?")) {
        await fetch(`/api/postactions/${id}`, { method: 'DELETE' });
        loadPostActionsTable();
    }
}

// -------------------------------------------------------------
// 8. LOGS
// -------------------------------------------------------------
async function loadLogsTable() {
    const res = await fetch('/api/logs?page=1&pageSize=50');
    const data = await res.json();
    const tbody = document.getElementById('logs-table-body');

    if (!data.logs || data.logs.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center">Sin registros de auditoría.</td></tr>`;
        return;
    }

    tbody.innerHTML = data.logs.map(l => `
        <tr>
            <td>#${l.id}</td>
            <td>${new Date(l.triggeredAt).toLocaleString()}</td>
            <td><strong>${escapeHtml(l.jobName)}</strong></td>
            <td>${getStatusBadge(l.status)}</td>
            <td>${escapeHtml(l.targetRecipient || '-')}</td>
            <td>${escapeHtml(l.renderedSubject || '-')}</td>
            <td>${l.durationMs} ms</td>
            <td>
                <button class="btn btn-sm btn-secondary" onclick="viewLogDetail(${l.id})"><i class="fa-solid fa-eye"></i> Detalle</button>
            </td>
        </tr>
    `).join('');
}

async function viewLogDetail(id) {
    const res = await fetch(`/api/logs/${id}`);
    const log = await res.json();

    const html = `
        <div style="font-size:14px;">
            <p><strong>Fecha/Hora:</strong> ${new Date(log.triggeredAt).toLocaleString()}</p>
            <p><strong>Job:</strong> ${escapeHtml(log.jobName)}</p>
            <p><strong>Estado:</strong> ${getStatusBadge(log.status)}</p>
            <p><strong>Destinatario:</strong> ${escapeHtml(log.targetRecipient || 'N/A')}</p>
            <p><strong>Filas de Datos Leídas:</strong> ${log.dataRowsFetched}</p>
            <p><strong>Duración:</strong> ${log.durationMs} ms</p>
            ${log.errorMessage ? `<div style="background:rgba(239,68,68,0.15); color:var(--accent-red); padding:10px; border-radius:6px; margin:10px 0;"><strong>Error:</strong> ${escapeHtml(log.errorMessage)}</div>` : ''}
            ${log.postExecutionDetails ? `<div style="background:rgba(16,185,129,0.15); color:var(--accent-green); padding:10px; border-radius:6px; margin:10px 0;"><strong>Acción Post-Ejecución:</strong> ${escapeHtml(log.postExecutionDetails)}</div>` : ''}
            <div style="margin-top:15px; background:rgba(0,0,0,0.3); padding:12px; border-radius:6px;">
                <strong>Asunto Renderizado:</strong><div>${escapeHtml(log.renderedSubject || '')}</div>
                <hr style="border-color:var(--border-color); margin:8px 0;">
                <strong>Cuerpo Renderizado:</strong>
                <div style="margin-top:5px; font-size:13px; max-height:200px; overflow:auto;">${log.renderedBody || ''}</div>
            </div>
        </div>
    `;
    showModal(`Detalle de Log #${log.id}`, html, false);
}

// -------------------------------------------------------------
// UTILS & MODAL ENGINE
// -------------------------------------------------------------
function showModal(title, bodyHtml, showSaveBtn = true, onSave = null) {
    document.getElementById('modal-title').textContent = title;
    document.getElementById('modal-body').innerHTML = bodyHtml;

    const saveBtn = document.getElementById('modal-save-btn');
    if (showSaveBtn && onSave) {
        saveBtn.style.display = 'inline-block';
        saveBtn.onclick = onSave;
    } else {
        saveBtn.style.display = 'none';
    }

    document.getElementById('modal-backdrop').classList.remove('hidden');
}

function closeModal() {
    document.getElementById('modal-backdrop').classList.add('hidden');
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
