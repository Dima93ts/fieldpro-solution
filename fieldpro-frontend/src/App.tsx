import { useEffect, useState, type FormEvent } from "react";
import "bootstrap/dist/css/bootstrap.min.css";

const API_BASE = import.meta.env.VITE_API_BASE;
const TENANT = import.meta.env.VITE_TENANT;

type Job = {
  id: number;
  code: string;
  customerName: string;
  address: string;
  scheduledAt: string;
  completedAt: string | null;
  status: string;
  project?: string;
  technicianId?: number;
  technicianName?: string | null;
  notes?: string | null;
};

type Technician = {
  id: number;
  name: string;
  email?: string;
};

type JobFormData = {
  code: string;
  customerName: string;
  address: string;
  scheduledAt: string;
  status: (typeof ALLOWED_STATUSES)[number];
  project: string;
  technicianId: number | undefined;
};

const ALLOWED_STATUSES = ["Scheduled", "InProgress", "Completed"] as const;

function App() {
  const [jobs, setJobs] = useState<Job[]>([]);
  const [technicians, setTechnicians] = useState<Technician[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [search, setSearch] = useState("");
  const [includeArchived, setIncludeArchived] = useState(false);

  const [formData, setFormData] = useState<JobFormData>({
    code: "",
    customerName: "",
    address: "",
    scheduledAt: "",
    status: "Scheduled",
    project: "",
    technicianId: undefined,
  });

  const [rowNotes, setRowNotes] = useState<Record<number, string>>({});

  useEffect(() => {
    loadTechnicians();
    loadJobs();
  }, []);

  const loadTechnicians = async () => {
    try {
      const res = await fetch(`${API_BASE}/technicians`, {
        headers: {
          "X-Tenant": TENANT,
        },
      });
      if (!res.ok) {
        console.error("Errore caricamento tecnici");
        return;
      }

      const data: Technician[] = await res.json();
      setTechnicians(data);
    } catch (err) {
      console.error("Errore rete caricando tecnici", err);
    }
  };

  const loadJobs = async (filters?: {
    status?: string;
    search?: string;
    includeArchived?: boolean;
  }) => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        page: "1",
        pageSize: "20",
      });
      if (filters?.status) params.set("status", filters.status);
      if (filters?.search) params.set("search", filters.search);
      if (filters?.includeArchived) params.set("includeArchived", "true");

      const res = await fetch(`${API_BASE}/jobs?${params.toString()}`, {
        headers: {
          "X-Tenant": TENANT,
        },
      });
      if (!res.ok) throw new Error("Errore caricamento jobs");
      const data: Job[] = await res.json();
      setJobs(data);
      setError("");
    } catch (err: any) {
      setError(err.message ?? "Errore generico");
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    if (
      !formData.code ||
      !formData.customerName ||
      !formData.address ||
      !formData.scheduledAt
    ) {
      alert("Compila tutti i campi obbligatori");
      return;
    }

    if (new Date(formData.scheduledAt) <= new Date()) {
      alert("La data deve essere futura");
      return;
    }

    try {
      const payload: any = {
        code: formData.code,
        customerName: formData.customerName,
        address: formData.address,
        scheduledAt: formData.scheduledAt,
        status: formData.status,
        project: formData.project || undefined,
      };
      if (formData.technicianId) {
        payload.technicianId = formData.technicianId;
      }

      const res = await fetch(`${API_BASE}/jobs`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Tenant": TENANT,
        },
        body: JSON.stringify(payload),
      });

      if (!res.ok) throw new Error("Errore creazione job");

      setFormData({
        code: "",
        customerName: "",
        address: "",
        scheduledAt: "",
        status: "Scheduled",
        project: "",
        technicianId: undefined,
      });

      await loadJobs({
        status: statusFilter || undefined,
        search: search || undefined,
        includeArchived,
      });
    } catch (err: any) {
      alert(err.message ?? "Errore creazione job");
    }
  };

  const handleArchive = async (id: number) => {
    if (!confirm("Confermi archiviazione?")) return;

    try {
      const res = await fetch(`${API_BASE}/jobs/${id}`, {
        method: "DELETE",
        headers: {
          "X-Tenant": TENANT,
        },
      });
      if (!res.ok) throw new Error("Errore archiviazione");
      await loadJobs({
        status: statusFilter || undefined,
        search: search || undefined,
        includeArchived,
      });
    } catch (err: any) {
      alert(err.message ?? "Errore archiviazione");
    }
  };

  const handleChangeStatus = async (job: Job, newStatus: string) => {
    if (job.status === newStatus) return;

    const notesToSend =
      newStatus === "Completed" ? rowNotes[job.id] ?? "" : job.notes ?? "";

    try {
      const res = await fetch(`${API_BASE}/jobs/${job.id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          "X-Tenant": TENANT,
        },
        body: JSON.stringify({
          status: newStatus,
          notes: notesToSend,
        }),
      });

      if (!res.ok) throw new Error("Errore aggiornamento status");

      if (newStatus === "Completed") {
        setRowNotes((prev) => {
          const clone = { ...prev };
          delete clone[job.id];
          return clone;
        });
      }

      await loadJobs({
        status: statusFilter || undefined,
        search: search || undefined,
        includeArchived,
      });
    } catch (err: any) {
      alert(err.message ?? "Errore aggiornamento status");
    }
  };

  const handleFiltersChange = (
    nextStatus: string,
    nextSearch: string,
    nextIncludeArchived: boolean
  ) => {
    setStatusFilter(nextStatus);
    setSearch(nextSearch);
    setIncludeArchived(nextIncludeArchived);
    loadJobs({
      status: nextStatus || undefined,
      search: nextSearch || undefined,
      includeArchived: nextIncludeArchived,
    });
  };

  const renderStatusBadge = (status: string) => {
    let cls = "badge ";
    if (status === "Completed") cls += "bg-success";
    else if (status === "InProgress") cls += "bg-warning text-dark";
    else cls += "bg-info";
    return <span className={cls}>{status}</span>;
  };

  return (
    <div className="container-fluid vh-100 p-0 d-flex flex-column">
      <header className="bg-primary text-white py-3">
        <div className="container d-flex justify-content-between align-items-center">
          <div>
            <h1 className="h3 mb-1">SiteOps Studio · FieldPro – Job Management</h1>
            <small>Soluzione demo per la gestione job e cantieri per tecnici sul campo</small>
          </div>
          <span className="badge bg-success">API online</span>
        </div>
      </header>

      <div className="container-fluid flex-grow-1 py-4">
        <div className="row h-100">
          {/* Colonna sinistra – Nuovo Job */}
          <div className="col-md-4 mb-3">
            <div className="card h-100">
              <div className="card-header">
                <h5 className="mb-0">Nuovo job</h5>
              </div>
              <div className="card-body">
                <form onSubmit={handleSubmit}>
                  <div className="mb-3">
                    <label className="form-label">Codice *</label>
                    <input
                      className="form-control"
                      value={formData.code}
                      onChange={(e) =>
                        setFormData({ ...formData, code: e.target.value })
                      }
                      minLength={3}
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Cliente *</label>
                    <input
                      className="form-control"
                      value={formData.customerName}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          customerName: e.target.value,
                        })
                      }
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Indirizzo *</label>
                    <input
                      className="form-control"
                      value={formData.address}
                      onChange={(e) =>
                        setFormData({ ...formData, address: e.target.value })
                      }
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Data programmata *</label>
                    <input
                      type="datetime-local"
                      className="form-control"
                      value={formData.scheduledAt}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          scheduledAt: e.target.value,
                        })
                      }
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Status</label>
                    <select
                      className="form-select"
                      value={formData.status}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          status: e.target
                            .value as (typeof ALLOWED_STATUSES)[number],
                        })
                      }
                    >
                      {ALLOWED_STATUSES.map((s) => (
                        <option key={s} value={s}>
                          {s}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Cantiere / Progetto</label>
                    <input
                      className="form-control"
                      value={formData.project}
                      onChange={(e) =>
                        setFormData({ ...formData, project: e.target.value })
                      }
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Tecnico</label>
                    <select
                      className="form-select"
                      value={formData.technicianId ?? ""}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          technicianId: e.target.value
                            ? parseInt(e.target.value, 10)
                            : undefined,
                        })
                      }
                    >
                      <option value="">Nessuno</option>
                      {technicians.map((t) => (
                        <option key={t.id} value={t.id}>
                          {t.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <button type="submit" className="btn btn-primary w-100">
                    Crea job
                  </button>
                </form>
              </div>
            </div>
          </div>

          {/* Colonna destra – Lista Jobs */}
          <div className="col-md-8">
            <div className="card h-100">
              <div className="card-header d-flex justify-content-between align-items-center">
                <h5 className="mb-0">Lista jobs</h5>
                <div className="d-flex align-items-center gap-2">
                  <select
                    className="form-select form-select-sm"
                    value={statusFilter}
                    onChange={(e) =>
                      handleFiltersChange(
                        e.target.value,
                        search,
                        includeArchived
                      )
                    }
                  >
                    <option value="">Tutti status</option>
                    {ALLOWED_STATUSES.map((s) => (
                      <option key={s} value={s}>
                        {s}
                      </option>
                    ))}
                  </select>
                  <input
                    className="form-control form-control-sm"
                    placeholder="Cerca codice/cliente/progetto..."
                    value={search}
                    onChange={(e) =>
                      handleFiltersChange(
                        statusFilter,
                        e.target.value,
                        includeArchived
                      )
                    }
                  />
                  <div className="form-check form-check-sm ms-2">
                    <input
                      id="chkIncludeArchived"
                      className="form-check-input"
                      type="checkbox"
                      checked={includeArchived}
                      onChange={(e) =>
                        handleFiltersChange(
                          statusFilter,
                          search,
                          e.target.checked
                        )
                      }
                    />
                    <label
                      className="form-check-label small"
                      htmlFor="chkIncludeArchived"
                    >
                      Includi archiviati
                    </label>
                  </div>
                </div>
              </div>
              <div className="card-body p-0">
                {loading ? (
                  <div className="p-4 text-center">Caricamento...</div>
                ) : error ? (
                  <div className="alert alert-danger m-3">{error}</div>
                ) : jobs.length === 0 ? (
                  <p className="text-center text-muted p-4 mb-0">
                    Nessun job trovato
                  </p>
                ) : (
                  <div className="table-responsive">
                    <table className="table table-hover mb-0">
                      <thead className="table-light">
                        <tr>
                          <th>Codice</th>
                          <th>Cliente</th>
                          <th>Progetto</th>
                          <th>Tecnico</th>
                          <th>Status</th>
                          <th>Data</th>
                          <th>Note</th>
                          <th>Azioni</th>
                        </tr>
                      </thead>
                      <tbody>
                        {jobs.map((j) => (
                          <tr key={j.id}>
                            <td>
                              <strong>{j.code}</strong>
                            </td>
                            <td>{j.customerName}</td>
                            <td>{j.project || "-"}</td>
                            <td>{j.technicianName || "-"}</td>
                            <td>{renderStatusBadge(j.status)}</td>
                            <td>
                              {new Date(j.scheduledAt).toLocaleDateString()}
                            </td>

                            <td
                              style={{
                                maxWidth: 240,
                                whiteSpace: "pre-wrap",
                              }}
                            >
                              {j.notes && j.notes.trim().length > 0
                                ? j.notes
                                : "-"}
                            </td>

                            <td>
                              <div className="d-flex flex-column gap-1">
                                {j.status !== "Completed" && (
                                  <textarea
                                    className="form-control form-control-sm"
                                    rows={2}
                                    placeholder="Note per il completamento..."
                                    value={rowNotes[j.id] ?? ""}
                                    onChange={(e) =>
                                      setRowNotes((prev) => ({
                                        ...prev,
                                        [j.id]: e.target.value,
                                      }))
                                    }
                                  />
                                )}

                                <div className="d-flex gap-1 align-items-center">
                                  <button
                                    type="button"
                                    className="btn btn-sm btn-outline-warning"
                                    disabled={j.status === "InProgress"}
                                    onClick={() =>
                                      handleChangeStatus(j, "InProgress")
                                    }
                                  >
                                    Start
                                  </button>
                                  <button
                                    type="button"
                                    className="btn btn-sm btn-outline-success"
                                    disabled={j.status === "Completed"}
                                    onClick={() =>
                                      handleChangeStatus(j, "Completed")
                                    }
                                  >
                                    Complete
                                  </button>
                                  <button
                                    type="button"
                                    className="btn btn-sm btn-outline-secondary"
                                    onClick={() => handleArchive(j.id)}
                                  >
                                    Archive
                                  </button>
                                </div>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
            <footer className="text-center text-muted py-3 mt-4" style={{ borderTop: '1px solid #dee2e6' }}>
                      <small>Powered by SiteOps Studio</small>
                    </footer>
    </div>
  );
}

export default App;
